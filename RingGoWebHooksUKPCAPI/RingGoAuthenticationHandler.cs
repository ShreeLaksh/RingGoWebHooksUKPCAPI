using System;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ANPRTechOps.RingGoIntegration.Web
{
    public class RingGoAuthenticationHandler : AuthenticationHandler<RingGoAuthenticationSchemeOptions>
    {
        private readonly ILogger<RingGoAuthenticationHandler> _logger;
        private const int AuthenticationTimestampTolerance = 11300;

        public const string AuthenticationSchemeName = "RingGo";

        public RingGoAuthenticationHandler(IOptionsMonitor<RingGoAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
            _logger = logger.CreateLogger<RingGoAuthenticationHandler>();
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Options.ValidateBearerHmac)
            {
                var noAuthIdentity = new ClaimsIdentity(new[] { new Claim("name", "anonymous") }, nameof(RingGoAuthenticationHandler));
                var noAuthTicket = new AuthenticationTicket(new ClaimsPrincipal(noAuthIdentity), Scheme.Name);
                return await Task.FromResult(AuthenticateResult.Success(noAuthTicket));
            }

            string authHeader = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authHeader)) return await Task.FromResult(AuthenticateResult.Fail("Invalid token"));

            _logger.LogTrace($"Authorization Header: {authHeader}");

            var body = await GetBodyAsText(Request);
            var bodyHash = GetHash(Options.PrivateKey, body);
            _logger.LogTrace($"Body: {body}");
            _logger.LogTrace($"Body Hash: {bodyHash}");

            var authHeaderParts = authHeader.Replace("Bearer ", string.Empty, StringComparison.OrdinalIgnoreCase).Split(":");
            if (authHeaderParts.Length != 4)
            {
                _logger.LogTrace("Invalid number of parts in authorization header");
                return await Task.FromResult(AuthenticateResult.Fail("Invalid token"));
            }

            var authPublicKey = authHeaderParts[0];
            var authTimestamp = Convert.ToInt32(authHeaderParts[1]);
            var authNonce = authHeaderParts[2];
            var authSignature = authHeaderParts[3];

            var signature = GetSignature(Options.PublicKey, Options.PrivateKey, authTimestamp, authNonce, $"{Request.Path}{Request.QueryString}", bodyHash);

            if (!string.Equals(Options.PublicKey, authPublicKey))
            {
                _logger.LogTrace($"Public Key Mismatch - {Options.PublicKey} vs {authPublicKey}");
                return await Task.FromResult(AuthenticateResult.Fail("Invalid token"));
            }

            var epochNow = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            if (Math.Abs(epochNow - authTimestamp) > AuthenticationTimestampTolerance)
            {
                _logger.LogTrace($"Timestamp too old - {epochNow - authTimestamp} seconds drift from now");
                return await Task.FromResult(AuthenticateResult.Fail("Invalid timestamp"));
            }

            if (!string.Equals(signature, authSignature))
            {
                _logger.LogTrace($"Signature Mismatch - {signature} vs {authSignature}");
                return await Task.FromResult(AuthenticateResult.Fail("Invalid signature"));
            }

            var identity = new ClaimsIdentity(new[] { new Claim("name", authHeaderParts[0]) }, nameof(RingGoAuthenticationHandler));
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
            return await Task.FromResult(AuthenticateResult.Success(ticket));
        }

        private string GetSignature(string publicKey, string privateKey, int timestamp, string nonce, string requestPath, string bodyHash)
        {
            var valueToHash = $"{publicKey}:{timestamp}:{nonce}:{requestPath}:{bodyHash}";
            return GetHash(privateKey, valueToHash);
        }

        private static async Task<string> GetBodyAsText(HttpRequest request)
        {
            request.EnableBuffering();
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            var bodyStream = request.BodyReader.AsStream(true);
            await bodyStream.ReadAsync(buffer, 0, buffer.Length);
            var body = Encoding.UTF8.GetString(buffer);

            request.Body.Seek(0, SeekOrigin.Begin);
            return body;
        }

        private static string GetHash(string privateKey, string contents)
        {
            var key = Convert.FromBase64String(privateKey);
            var hmac = new HMACSHA256(key);
            var data = hmac.ComputeHash(Encoding.UTF8.GetBytes(contents));
            return Convert.ToBase64String(data);

            //using var sha256Hash = SHA256.Create();
            //var data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(privateKey + contents));

            var sb = new StringBuilder();

            foreach (var t in data)
            {
                sb.Append(t.ToString("x2"));
            }

            return sb.ToString();
        }
    }

    public class RingGoAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public string? PublicKey { get; set; }

        public string? PrivateKey { get; set; }

        public bool ValidateBearerHmac { get; set; }
    }
}