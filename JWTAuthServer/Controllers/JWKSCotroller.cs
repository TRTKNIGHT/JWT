using JWTAuthServer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace JWTAuthServer.Controllers
{
    [Route(".well-known")]
    [ApiController]
    public class JWKSController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JWKSController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("jwks.json")]
        public IActionResult GetJWKS()
        {
            var activeKeys = _context.SigningKeys
                .Where(k => k.IsActive)
                .ToList();
            var jwks = new
            {
                keys = activeKeys.Select(k => new
                {
                    kty = "RSA",
                    use = "sig",
                    kid = k.KeyId,
                    alg = "RS512",
                    n = Base64UrlEncoder.Encode(GetModulus(k.PublicKey)),
                    e = Base64UrlEncoder.Encode(GetExponent(k.PublicKey))
                })
            };

            return Ok(jwks);
        }

        private byte[] GetModulus(string publicKey)
        {
            var rsa = RSA.Create();
            var publicKeyBytes = Convert.FromBase64String(publicKey);
            rsa.ImportRSAPublicKey(publicKeyBytes, out _);

            var parameters = rsa.ExportParameters(false);
            rsa.Dispose();

            if (parameters.Modulus is null)
                throw new InvalidOperationException("RSA parametrs not valid");
            return parameters.Modulus;
        }

        private byte[] GetExponent(string publicKey)
        {
            var rsa = RSA.Create();
            var publicKeyBytes = Convert.FromBase64String(publicKey);
            rsa.ImportRSAPublicKey(publicKeyBytes, out _);

            var parameters = rsa.ExportParameters(false);
            rsa.Dispose();

            if (parameters.Exponent is null)
                throw new InvalidOperationException("RSA parameters not valid");
            return parameters.Exponent;
        }
    }
}
