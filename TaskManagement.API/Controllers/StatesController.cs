using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers;

/// <summary>
/// Controlador para la gestión de estados de las tareas
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[Tags("📊 Estados de Tareas")]
public class StatesController : ControllerBase
{
    private readonly IStateService _stateService;

    public StatesController(IStateService stateService)
    {
        _stateService = stateService;
    }

    /// <summary>
    /// Obtiene todos los estados disponibles en el sistema
    /// </summary>
    /// <returns>Lista completa de estados de tareas</returns>
    /// <remarks>
    /// Los estados representan el ciclo de vida de una tarea, por ejemplo:
    /// - Pendiente
    /// - En Progreso  
    /// - Completada
    /// - Cancelada
    /// 
    /// Este endpoint no requiere parámetros y retorna todos los estados activos.
    /// </remarks>
    /// <response code="200">Lista de estados obtenida exitosamente</response>
    /// <response code="401">Token JWT requerido</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StateDto>), 200)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<IEnumerable<StateDto>>> GetStates()
    {
        try
        {
            var states = await _stateService.GetAllAsync();
            return Ok(states);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene un estado específico por su ID
    /// </summary>
    /// <param name="id">ID único del estado a consultar</param>
    /// <returns>Información detallada del estado solicitado</returns>
    /// <remarks>
    /// Ejemplo de uso: GET /api/States/1
    /// 
    /// Útil para obtener información específica de un estado antes de asignarlo a una tarea.
    /// </remarks>
    /// <response code="200">Estado encontrado exitosamente</response>
    /// <response code="404">Estado no encontrado con el ID especificado</response>
    /// <response code="401">Token JWT requerido</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(StateDto), 200)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<StateDto>> GetState(int id)
    {
        try
        {
            var state = await _stateService.GetByIdAsync(id);
            if (state == null)
            {
                return NotFound(new { message = $"Estado con ID {id} no encontrado" });
            }
            return Ok(state);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Crea un nuevo estado en el sistema
    /// </summary>
    /// <param name="createStateDto">Información del nuevo estado a crear</param>
    /// <returns>Estado creado con su ID asignado</returns>
    /// <remarks>
    /// Ejemplo de uso:
    /// 
    ///     POST /api/States
    ///     {
    ///         "name": "En Revisión",
    ///         "description": "Tarea pendiente de revisión por supervisor"
    ///     }
    /// 
    /// Requisitos:
    /// - Name: requerido, no puede estar vacío
    /// - El nombre debe ser único en el sistema
    /// </remarks>
    /// <response code="201">Estado creado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="409">Ya existe un estado con ese nombre</response>
    /// <response code="401">Token JWT requerido</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPost]
    [ProducesResponseType(typeof(StateDto), 201)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 409)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<StateDto>> CreateState([FromBody] CreateStateDto createStateDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(createStateDto.Name))
            {
                return BadRequest(new { message = "El nombre del estado es requerido" });
            }

            var createdState = await _stateService.CreateAsync(createStateDto);
            return CreatedAtAction(nameof(GetState), new { id = createdState.Id }, createdState);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza un estado existente
    /// </summary>
    /// <param name="id">ID del estado a actualizar</param>
    /// <param name="updateStateDto">Nuevos datos del estado</param>
    /// <returns>Estado actualizado</returns>
    /// <remarks>
    /// Ejemplo de uso:
    /// 
    ///     PUT /api/States/1
    ///     {
    ///         "name": "Completada",
    ///         "description": "Tarea finalizada exitosamente"
    ///     }
    /// 
    /// Nota: Solo se pueden actualizar estados que no estén siendo utilizados por tareas activas.
    /// </remarks>
    /// <response code="200">Estado actualizado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="404">Estado no encontrado</response>
    /// <response code="409">Conflicto - nombre duplicado o estado en uso</response>
    /// <response code="401">Token JWT requerido</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(StateDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(object), 409)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<StateDto>> UpdateState(int id, [FromBody] UpdateStateDto updateStateDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(updateStateDto.Name))
            {
                return BadRequest(new { message = "El nombre del estado es requerido" });
            }

            var updatedState = await _stateService.UpdateAsync(id, updateStateDto);
            return Ok(updatedState);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Elimina un estado del sistema
    /// </summary>
    /// <param name="id">ID del estado a eliminar</param>
    /// <returns>Confirmación de eliminación</returns>
    /// <remarks>
    /// ⚠️ **Advertencia**: Esta operación es irreversible.
    /// 
    /// Solo se pueden eliminar estados que:
    /// - No estén asignados a ninguna tarea
    /// - No sean estados del sistema (como "Pendiente" o "Completada")
    /// 
    /// Ejemplo: DELETE /api/States/5
    /// </remarks>
    /// <response code="204">Estado eliminado exitosamente</response>
    /// <response code="404">Estado no encontrado</response>
    /// <response code="409">No se puede eliminar - estado en uso por tareas</response>
    /// <response code="401">Token JWT requerido</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(object), 409)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> DeleteState(int id)
    {
        try
        {
            var result = await _stateService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"Estado con ID {id} no encontrado" });
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }
}