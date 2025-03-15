using JWTAuthServer.Data;
using JWTAuthServer.DTOs;
using JWTAuthServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JWTAuthServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Constructor injecting the ApplicationDbContext
        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Registers a new user.
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == registerDto.Email.ToLower());
            if (existingUser is not null)
                return Conflict();

            var passwordHash = BCrypt.Net.BCrypt
                .HashPassword(registerDto.Password);

            var user = new User
            {
                Email = registerDto.Email,
                Firstname = registerDto.Firstname,
                Lastname = registerDto.Lastname,
                Password = passwordHash
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var role = await _context.Roles
                .FirstOrDefaultAsync(r =>
                    r.Name == "User");
            if (role is not null)
            {
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id
                };
                await _context.UserRoles.AddAsync(userRole);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetProfile),
                new { id = user.Id },
                new { message = "User registered successfully" }
            );
        }

        // Retrieves the authenticated user's profile.
        [HttpGet("GetProfile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var emailClaim = User.Claims.FirstOrDefault(c =>
                c.Type == System.Security.Claims.ClaimTypes.Email
            );
            if (emailClaim is null)
                return Unauthorized(new { message = "Email claim not found" });

            var email = emailClaim.Value;
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => 
                    u.Email == email);
            if (user is null)
                return NotFound(new { message = "User not found" });

            var profile = new ProfileDTO
            {
                Id = user.Id,
                Email = email,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            };

            return Ok(profile);
        }

        // Updates the authenticated user's profile.
        [HttpPut("UpdateProfile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO updateDto)
        {
            // Validate the incoming model.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Extract the user's email from the JWT token claims.
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email);
            if (emailClaim == null)
            {
                return Unauthorized(new { message = "Invalid token: Email claim missing." });
            }

            string userEmail = emailClaim.Value;

            // Retrieve the user from the database.
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == userEmail.ToLower());

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            user.Firstname = updateDto.Firstname;
            user.Lastname = updateDto.Lastname;
            user.Email = updateDto.Email;

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(updateDto.Password);
            user.Password = hashedPassword;

            // Save the changes to the database.
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully." });
        }
    }
}
