using Domain.Models.Read;
using Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Application.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;

    public AuthController(IAuthService authService, IUserRepository userRepository)
    {
        _authService = authService;
        _userRepository = userRepository;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDTO>> Register([FromBody] UserRegistrationDTO registrationDto)
    {
        try
        {
            var registrationResponse = await _authService.RegisterAsync(registrationDto);
            return Ok(registrationResponse);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDTO>> Login([FromBody] UserLoginDTO loginDto)
    {
        var loginResponse = await _authService.LoginAsync(loginDto);
        if (loginResponse == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        return Ok(loginResponse);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDTO>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var currentUser = await _userRepository.GetByIdAsync(userId);
        if (currentUser == null)
        {
            return NotFound();
        }

        var currentUserDto = new UserDTO
        {
            Id = currentUser.Id,
            Email = currentUser.Email,
            FirstName = currentUser.FirstName,
            LastName = currentUser.LastName,
            CreatedAt = currentUser.CreatedAt
        };

        return Ok(currentUserDto);
    }
}