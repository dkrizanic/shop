using Domain.Models;
using Domain.Models.Read;

namespace Domain.Repositories;

public interface IAuthService
{
    Task<AuthResponseDTO> RegisterAsync(UserRegistrationDTO registrationDto);
    Task<AuthResponseDTO?> LoginAsync(UserLoginDTO loginDto);
    string GenerateJwtToken(User user);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}