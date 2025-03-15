using JWTAuthServer.Data;
using JWTAuthServer.DTOs;
using JWTAuthServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JWTAuthServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private ApplicationDbContext _context;
        private IConfiguration _configuration;

        public AuthController(
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var client = await _context.Clients
                .FirstOrDefaultAsync(c =>
                    c.ClientId == loginDto.ClientId);
            if (client is null) 
                return Unauthorized("Client not allowed");

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == loginDto.Email.ToLower());
            if (user is null) 
                return Unauthorized("User not found");

            bool isValidPassword = BCrypt.Net.BCrypt
                .Verify(loginDto.Password, user.Password);
            if (!isValidPassword)
                return Unauthorized("Incorrect Password");

            var token = GenerateJwtToken(user, client);
            var refreshToken = GenerateRefreshToken();
            var hashedRefreshToken = HashToken(refreshToken);

            var refreshTokenEntity = new RefreshToken
            {
                Token = hashedRefreshToken,
                UserId = user.Id,
                ClientId = client.Id,
                ExpiresAt = DateTime.Now.AddDays(7),
                CreatedAt = DateTime.Now,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return Ok(new TokenResponseDTO
            {
                Token = token,
                RefreshToken = refreshToken
            });
        }

        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDTO logoutRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim is null)
                return Unauthorized("Invalid access token");

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized("Invalid user ID in access token");

            var hashedRefreshToken = HashToken(logoutRequest.RefreshToken);
            var storedRefreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .Include(rt => rt.Client)
                .FirstOrDefaultAsync(rt =>
                    rt.Token == hashedRefreshToken &&
                    rt.Client.ClientId == logoutRequest.ClientId);

            if (storedRefreshToken is null)
                return Unauthorized("Invalid refresh token");
            if (storedRefreshToken.IsRevoked)
                return Unauthorized("Refresh token has been revoked");

            storedRefreshToken.IsRevoked = true;
            storedRefreshToken.RevokedAt = DateTime.Now;

            if (logoutRequest.IsLogoutFromAllDevices)
            {
                var userRefreshTokens = await _context.RefreshTokens
                    .Where(rt =>
                        rt.UserId == storedRefreshToken.UserId &&
                        !rt.IsRevoked)
                    .ToListAsync();
                foreach (var token in userRefreshTokens)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new
            {
                Message = "Logout successful. Refresh token has been revoked."
            });
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var hashedToken = HashToken(requestDto.RefreshToken);

            var storedRefreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .Include(rt => rt.Client)
                .FirstOrDefaultAsync(rt =>
                    rt.Token == hashedToken &&
                    rt.Client.ClientId == requestDto.ClientId);

            if (storedRefreshToken is null)
                return Unauthorized("Invalid refresh token");
            if (storedRefreshToken.IsRevoked)
                return Unauthorized("Refresh token has been revoked");
            if (storedRefreshToken.ExpiresAt < DateTime.Now)
                return Unauthorized("Refresh token has been expired");

            var user = storedRefreshToken.User;
            var client = storedRefreshToken.Client;

            storedRefreshToken.IsRevoked = true;
            storedRefreshToken.RevokedAt = DateTime.Now;

            var tokenString = GenerateRefreshToken();
            var hashedTokenString = HashToken(tokenString);

            var newRefreshToken = new RefreshToken
            {
                Token = hashedTokenString,
                UserId = user.Id,
                ClientId = client.Id,
                ExpiresAt = DateTime.Now.AddDays(7),
                IsRevoked = false,
            };

            _context.RefreshTokens.Add(newRefreshToken);
            var newJwtToken = GenerateJwtToken(user, client);
            await _context.SaveChangesAsync();

            return Ok(new TokenResponseDTO
            {
                Token = newJwtToken,
                RefreshToken = hashedTokenString
            });
        }

        private string GenerateJwtToken(User user, Client client)
        {
            var activeKey = _context.SigningKeys
                .FirstOrDefault(s => s.IsActive);

            if (activeKey is null)
                throw new Exception("No active signing key available");

            var privateKeyBytes =
                Convert.FromBase64String(activeKey.PrivateKey);

            var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(privateKeyBytes, out _);

            var rsaSecureKey = new RsaSecurityKey(rsa)
            {
                KeyId = activeKey.KeyId
            };
            var creds = new
                SigningCredentials(rsaSecureKey, SecurityAlgorithms.RsaSha512);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.Firstname),
                new Claim(ClaimTypes.NameIdentifier, user.Email),
                new Claim(ClaimTypes.Email, user.Email)
            };
            foreach (var userRole in user.UserRoles)
            {
                claims.Add(
                    new Claim(ClaimTypes.Role, userRole.Role.Name));
            }

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: client.ClientURL,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.WriteToken(tokenDescriptor);
            rsa.Dispose();
            return token;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private string HashToken(string token)
        {
            using var sha512 = SHA512.Create();
            var hashedBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
