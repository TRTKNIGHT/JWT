using AutoMapper;
using Demo0.DTOs;
using Demo0.EF;
using Demo0.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demo0.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public UserController(
        AppDbContext context,
        IMapper mapper
    )
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userExists = await _context.Users.FirstOrDefaultAsync(
            u => u.Email!.ToLower() == model.Email.ToLower()
        );
        if (userExists is not null)
        {
            return Conflict(new { message = "User already created" });
        }

        var hashedPassword = BCrypt.Net.BCrypt
            .HashPassword(model.Password);

        var newUser = _mapper.Map<User>(model);
        newUser.Password = hashedPassword;

        await _context.Users.AddAsync(newUser);
        await _context.SaveChangesAsync();

        var userRole = await _context.Roles.FirstOrDefaultAsync(
            r => r.Name == "User"
        );

        if (userRole is not null)
        {
            var newUserRole = new UserRole
            {
                UserId = newUser.Id,
                RoleId = userRole.Id
            };
            await _context.UserRoles.AddAsync(newUserRole);
            await _context.SaveChangesAsync();
        }

        return CreatedAtAction(
            nameof(GetProfile),
            new { userId = newUser.Id },
            new { message = "User registered successfully" }
        );
    }

    [HttpGet("GetProfile")]
    [Authorize]
    public async Task<IActionResult> GetProfile(int userId)
    {
        var emailClaim = User.Claims.FirstOrDefault(
            c => c.Type == System.Security.Claims.ClaimTypes.Email
        );

        if (emailClaim is null)
        {
            return Unauthorized(
                new { message = "Invalid claim: Email missing" }
            );
        }

        string userEmail = emailClaim.Value;
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(
                u => u.Email.ToLower() == userEmail.ToLower()
            );

        if (user is null)
        {
            return NotFound(
                new { message = "User not found" }
            );
        }

        var profile = _mapper.Map<ProfileDto>(user);

        return Ok(profile);
    }

    [HttpPut("UpdateProfile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateDto updateDto)
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

        _mapper.Map(updateDto, user);
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(updateDto.Password);
        user.Password = hashedPassword;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Profile updated successfully." });
    }
}
