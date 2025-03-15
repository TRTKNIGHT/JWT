using AutoMapper;
using IdentityIntegration.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityIntegration.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountsController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signinManager;
    //private readonly RoleManager<IdentityUser> _roleManager;
    private readonly IMapper _mapper;

    public AccountsController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signinManager,
        //RoleManager<IdentityUser> roleManager,
        IMapper mapper)
    {
        _userManager = userManager;
        _signinManager = signinManager;
        //_roleManager = roleManager;
        _mapper = mapper;
    }

    [HttpPost("")]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = _mapper.Map<IdentityUser>(registerDto);
        var result = await _userManager.CreateAsync(
            user, registerDto.Password
        );

        if (result.Succeeded)
        {
            await _signinManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction();
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return BadRequest(ModelState);
    }
}
