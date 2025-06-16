using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.Services;

namespace TaskManagement.API.Controllers
{
    /// <summary>
    /// Controlador para la gestión de autenticación y autorización de usuarios
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Tags("🔐 Autenticación")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Inicia sesión de un usuario existente en el sistema
        /// </summary>
        /// <param name="loginDto">Credenciales de acceso del usuario (email y contraseña)</param>
        /// <returns>Token JWT y información del usuario autenticado</returns>
        /// <remarks>
        /// Ejemplo de uso:
        /// 
        ///     POST /api/Auth/login
        ///     {
        ///         "email": "usuario@ejemplo.com",
        ///         "password": "miContraseña123"
        ///     }
        /// 
        /// </remarks>
        /// <response code="200">Inicio de sesión exitoso - Retorna token JWT</response>
        /// <response code="401">Credenciales inválidas - Email o contraseña incorrectos</response>
        /// <response code="400">Error en los datos enviados</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var response = await _authService.LoginAsync(loginDto);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema
        /// </summary>
        /// <param name="registerDto">Información del nuevo usuario a registrar</param>
        /// <returns>Token JWT y información del usuario registrado</returns>
        /// <remarks>
        /// Ejemplo de uso:
        /// 
        ///     POST /api/Auth/register
        ///     {
        ///         "username": "nuevoUsuario",
        ///         "email": "nuevo@ejemplo.com",
        ///         "password": "miContraseña123"
        ///     }
        /// 
        /// Requisitos:
        /// - Username: mínimo 3 caracteres, máximo 50
        /// - Email: formato válido de email
        /// - Password: mínimo 6 caracteres
        /// </remarks>
        /// <response code="200">Usuario registrado exitosamente - Retorna token JWT</response>
        /// <response code="400">Error en los datos o usuario ya existe</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var response = await _authService.RegisterAsync(registerDto);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        /// <summary>
        /// Verifica si un email ya está registrado en el sistema
        /// </summary>
        /// <param name="email">Dirección de email a verificar</param>
        /// <returns>Indica si el email ya existe en el sistema</returns>
        /// <remarks>
        /// Útil para validaciones en tiempo real durante el registro.
        /// 
        /// Ejemplo: GET /api/Auth/check-email/usuario@ejemplo.com
        /// </remarks>
        /// <response code="200">Verificación completada - Retorna si el email existe</response>
        /// <response code="400">Error en la verificación</response>
        [HttpGet("check-email/{email}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<ActionResult<bool>> CheckEmail(string email)
        {
            try
            {
                var exists = await _authService.UserExistsAsync(email);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint temporal para debug - ELIMINAR EN PRODUCCIÓN
        /// </summary>
        [HttpPost("debug-register")]
        public IActionResult DebugRegister([FromBody] RegisterDto registerDto)
        {
            return Ok(new 
            { 
                received = new 
                {
                    registerDto_is_null = registerDto == null,
                    username = registerDto?.Username ?? "NULL",
                    email = registerDto?.Email ?? "NULL", 
                    password = registerDto?.Password ?? "NULL",
                    password_length = registerDto?.Password?.Length ?? 0,
                    username_length = registerDto?.Username?.Length ?? 0,
                    email_length = registerDto?.Email?.Length ?? 0
                },
                model_state_valid = ModelState.IsValid,
                model_state_errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage))
            });
        }
    }
}