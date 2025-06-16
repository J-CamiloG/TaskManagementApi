// using Microsoft.Extensions.Configuration;
// using Microsoft.IdentityModel.Tokens;
// using System.IdentityModel.Tokens.Jwt;
// using System.Security.Claims;
// using System.Text;
// using TaskManagement.Application.Services;
// using TaskManagement.Domain.Entities;

// namespace TaskManagement.Infrastructure.Services
// {
//     public class JwtService : IJwtService
//     {
//         private readonly IConfiguration _configuration;

//         public JwtService(IConfiguration configuration)
//         {
//             _configuration = configuration;
//         }

//         public string GenerateToken(User user)
//         {
//             var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
//             var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//             var claims = new[]
//             {
//                 new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
//                 new Claim(ClaimTypes.Name, user.Username),
//                 new Claim(ClaimTypes.Email, user.Email)
//             };

//             var token = new JwtSecurityToken(
//                 issuer: _configuration["Jwt:Issuer"],
//                 audience: _configuration["Jwt:Audience"],
//                 claims: claims,
//                 expires: DateTime.UtcNow.AddHours(24),
//                 signingCredentials: credentials
//             );

//             return new JwtSecurityTokenHandler().WriteToken(token);
//         }

//         public bool ValidateToken(string token)
//         {
//             try
//             {
//                 var tokenHandler = new JwtSecurityTokenHandler();
//                 var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

//                 tokenHandler.ValidateToken(token, new TokenValidationParameters
//                 {
//                     ValidateIssuerSigningKey = true,
//                     IssuerSigningKey = new SymmetricSecurityKey(key),
//                     ValidateIssuer = true,
//                     ValidIssuer = _configuration["Jwt:Issuer"],
//                     ValidateAudience = true,
//                     ValidAudience = _configuration["Jwt:Audience"],
//                     ValidateLifetime = true,
//                     ClockSkew = TimeSpan.Zero
//                 }, out SecurityToken validatedToken);

//                 return true;
//             }
//             catch
//             {
//                 return false;
//             }
//         }
//     }
// }
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string GenerateToken(User user)
        {
            try
            {
                _logger.LogInformation("=== GENERANDO TOKEN JWT ===");
                _logger.LogInformation("User ID: {UserId}, Username: {Username}, Email: {Email}", 
                    user.Id, user.Username, user.Email);

                // ✅ LEER VARIABLES DE ENTORNO PRIMERO, LUEGO APPSETTINGS
                var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") 
                    ?? _configuration["Jwt:Key"];
                var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
                    ?? _configuration["Jwt:Issuer"];
                var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
                    ?? _configuration["Jwt:Audience"];

                _logger.LogInformation("JWT Config - Key: {HasKey}, Issuer: {Issuer}, Audience: {Audience}", 
                    !string.IsNullOrEmpty(jwtKey), jwtIssuer, jwtAudience);

                if (string.IsNullOrEmpty(jwtKey))
                {
                    _logger.LogError("JWT Key is null or empty");
                    throw new InvalidOperationException("JWT Key no está configurada");
                }

                if (string.IsNullOrEmpty(jwtIssuer))
                {
                    _logger.LogError("JWT Issuer is null or empty");
                    throw new InvalidOperationException("JWT Issuer no está configurada");
                }

                if (string.IsNullOrEmpty(jwtAudience))
                {
                    _logger.LogError("JWT Audience is null or empty");
                    throw new InvalidOperationException("JWT Audience no está configurada");
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                _logger.LogInformation("Claims creados: {ClaimCount}", claims.Length);

                var token = new JwtSecurityToken(
                    issuer: jwtIssuer,
                    audience: jwtAudience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(24),
                    signingCredentials: credentials
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                
                _logger.LogInformation("Token generado exitosamente. Length: {Length}", tokenString?.Length ?? 0);
                
                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando token JWT para usuario {UserId}", user.Id);
                throw;
            }
        }

        public bool ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                
                // ✅ USAR VARIABLES DE ENTORNO PRIMERO
                var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") 
                    ?? _configuration["Jwt:Key"];
                var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
                    ?? _configuration["Jwt:Issuer"];
                var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
                    ?? _configuration["Jwt:Audience"];

                if (string.IsNullOrEmpty(jwtKey))
                    return false;

                var key = Encoding.UTF8.GetBytes(jwtKey);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return false;
            }
        }
    }
}