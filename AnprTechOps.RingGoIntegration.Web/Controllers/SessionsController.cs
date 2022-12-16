using System;
using System.Data.SqlClient;
using System.Net;
using System.Threading.Tasks;
using ANPRTechOps.RingGoIntegration.Web.Models;
using ANPRTechOps.RingGoIntegration.Web.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ANPRTechOps.RingGoIntegration.Web.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = RingGoAuthenticationHandler.AuthenticationSchemeName)]
    //***Editing route by adding openapi***
    [Route("UKPCAPI/[sessions]")]
    public class SessionsController : ControllerBase
    {
        private readonly IRingGoRepository _repository;
        private readonly ILogger<SessionsController> _logger;

        public SessionsController(IRingGoRepository repository, ILogger<SessionsController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SessionModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogTrace("Error - Invalid create session request");
                return new JsonResult(new { code = 2, message = "Invalid create session request" }) { StatusCode = (int)HttpStatusCode.BadRequest };
            }

            if (model.Parking.PaymentPence < 0)
            {
                _logger.LogTrace($"Error - Invalid payment value: {model.Parking.PaymentPence}");
                return new JsonResult(new { code = 3, message = "Invalid payment value" }) { StatusCode = (int)HttpStatusCode.BadRequest };
            }

            try
            {
                var sessionId = await _repository.InsertRingGoSession(model);
                _logger.LogTrace($"Added session with reference {model.RingGoReference} as Id {sessionId}");
                var result = new JsonResult(new { sessionId }) { StatusCode = (int)HttpStatusCode.Created };
                return result;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    _logger.LogTrace($"Error - Reference is already used: {model.RingGoReference}");
                    return new JsonResult(new { code = 4, message = "Reference is already used" }) { StatusCode = (int)HttpStatusCode.BadRequest };
                }

                _logger.LogError(ex, "Error - SQL Exception caught while adding session");
                return new JsonResult(new { code = 1, message = "Unable to create a session" }) { StatusCode = (int)HttpStatusCode.BadRequest };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error - Error caught while adding session");
                return new JsonResult(new { code = 1, message = "Unable to create a session" }) { StatusCode = (int)HttpStatusCode.BadRequest };
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(string ringgoRef)
        {
            _logger.LogTrace($"Get request for Reference {ringgoRef}");

            var session = await _repository.FindRingGoSessionByReference(ringgoRef);
            if (session == null) return new JsonResult(new object[0]);

            var model = session.AsSessionModel();
            return new JsonResult(new[] { model });

        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation($"Delete request for Session Id {id}");

            var success = await _repository.DeleteRingGoSession(id);
            return success ? (IActionResult)new NoContentResult() : new NotFoundResult();
        }
    }
}