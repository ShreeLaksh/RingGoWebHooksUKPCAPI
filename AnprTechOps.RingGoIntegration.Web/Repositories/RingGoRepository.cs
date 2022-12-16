using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using ANPRTechOps.RingGoIntegration.Web.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ANPRTechOps.RingGoIntegration.Web.Repositories
{
    public class RingGoRepository : IRingGoRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<RingGoRepository> _logger;
        private readonly RingGoSettings _settings;

        public RingGoRepository(IConfiguration configuration, ILogger<RingGoRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("AnprTechOpsDatabase");
            _settings = configuration.GetSection("RingGo").Get<RingGoSettings>();
            _logger = logger;
        }

        public async Task<int> InsertRingGoSession(SessionModel model)
        {
            try
            {
                await using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                var sessionId = await conn.ExecuteScalarAsync<int>("pp.usp_InsertRingGoSession",
                    new
                    {
                        _settings.VendorId,
                        Reference = model.RingGoReference,
                        model.Vehicle.Plate,
                        model.Vehicle.Make,
                        model.Vehicle.Colour,
                        model.Vehicle.Type,
                        model.Parking.Zone,
                        model.Parking.StartTime,
                        model.Parking.EndTime,
                        model.Parking.PaymentPence,
                        model.Parking.PermitType,
                        model.Parking.DisabledBadge,
                        model.Parking.OnWhiteList,
                        model.Parking.Notes,
                        model.Parking.Bay,
                        model.Location.CountryCode,
                        model.Location.OperatorId
                    }, commandType: CommandType.StoredProcedure, commandTimeout: 30);

                _logger.LogTrace($"Added RingGo Session for reference {model.RingGoReference} with Plate {model.Vehicle.Plate}");

                await conn.CloseAsync();
                return sessionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error caught when attempting to insert RingGo Session for reference {model.RingGoReference} with Plate {model.Vehicle.Plate}");
                throw;
            }
        }

        public async Task<SessionDto> FindRingGoSessionByReference(string reference)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var session = await conn.QueryFirstOrDefaultAsync<SessionDto>("pp.usp_FindRingGoSessionByReference", new { _settings.VendorId, Reference = reference }, commandType: CommandType.StoredProcedure, commandTimeout: 30);

            await conn.CloseAsync();
            return session;
        }

        public async Task<bool> DeleteRingGoSession(int id)
        {
            try
            {
                await using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                var rowsAffected = await conn.ExecuteAsync("pp.usp_DeleteRingGoSession", new { _settings.VendorId, Id = id }, commandType: CommandType.StoredProcedure, commandTimeout: 30);

                await conn.CloseAsync();

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error caught when attempting to delete RingGo Session for Id {id}");
                throw;
            }
        }
    }
}