using TaskManagement.Application.DTOs.Auth;

namespace TaskManagement.Application.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<bool> UserExistsAsync(string email);
    }
}