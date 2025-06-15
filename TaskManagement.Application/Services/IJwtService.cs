using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        bool ValidateToken(string token);
    }
}