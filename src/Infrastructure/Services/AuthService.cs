using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Domain.Models;
using Domain.Models.Read;
using Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponseDTO> RegisterAsync(UserRegistrationDTO registrationDto)
    {
        if (await _userRepository.EmailExistsAsync(registrationDto.Email))
        {
            throw new ArgumentException("Email already exists");
        }

        var user = new User
        {
            Email = registrationDto.Email,
            PasswordHash = HashPassword(registrationDto.Password),
            FirstName = registrationDto.FirstName,
            LastName = registrationDto.LastName
        };

        var createdUser = await _userRepository.CreateAsync(user);
        var token = GenerateJwtToken(createdUser);

        return new AuthResponseDTO
        {
            Token = token,
            User = new UserDTO
            {
                Id = createdUser.Id,
                Email = createdUser.Email,
                FirstName = createdUser.FirstName,
                LastName = createdUser.LastName,
                CreatedAt = createdUser.CreatedAt
            }
        };
    }

    public async Task<AuthResponseDTO?> LoginAsync(UserLoginDTO loginDto)
    {
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            return null;
        }

        var token = GenerateJwtToken(user);

        return new AuthResponseDTO
        {
            Token = token,
            User = new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedAt = user.CreatedAt
            }
        };
    }

    public string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
        var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}