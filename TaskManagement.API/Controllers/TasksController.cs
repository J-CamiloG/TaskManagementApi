using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers;

/// <summary>
/// Controlador principal para la gestión completa de tareas del sistema
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[Tags("📋 Gestión de Tareas")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Obtiene una lista paginada de tareas con filtros opcionales
    /// </summary>
    /// <param name="page">Número de página (por defecto: 1)</param>
    /// <param name="pageSize">Cantidad de elementos por página (1-100, por defecto: 10)</param>
    /// <param name="stateId">Filtrar por ID de estado específico (opcional)</param>
    /// <param name="dueDate">Filtrar por fecha de vencimiento (opcional)</param>
    /// <returns>Lista paginada de tareas que coinciden con los filtros</returns>
    /// <remarks>
    /// Este endpoint permite obtener tareas de forma paginada y aplicar filtros para búsquedas específicas.
    /// 
    /// Ejemplos de uso:
    /// - GET /api/Tasks - Obtiene las primeras 10 tareas
    /// - GET /api/Tasks?page=2&amp;pageSize=20 - Segunda página con 20 elementos
    /// - GET /api/Tasks?stateId=1 - Solo tareas con estado ID 1
    /// - GET /api/Tasks?dueDate=2024-12-31 - Tareas que vencen en esa fecha
    /// - GET /api/Tasks?page=1&amp;stateId=2&amp;dueDate=2024-12-25 - Combinando filtros
    /// 
    /// Límites:
    /// - Página mínima: 1
    /// - PageSize máximo: 100
    /// - PageSize mínimo: 1
    /// </remarks>
    /// <response code="200">Lista paginada de tareas obtenida exitosamente</response>
    /// <response code="401">Token JWT requerido</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<TaskDto>), 200)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<PagedResultDto<TaskDto>>> GetTasks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? stateId = null,
        [FromQuery] DateTime? dueDate = null)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var tasks = await _taskService.GetAllAsync(page, pageSize, stateId, dueDate);
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene una tarea específica con todos sus detalles
    /// </summary>
    /// <param name="id">ID único de la tarea a consultar</param>
    /// <returns>Información completa de la tarea incluyendo estado y fechas</returns>
    /// <remarks>
    /// Ejemplo de uso: GET /api/Tasks/123
    /// 
    /// Retorna información detallada incluyendo:
    /// - Título y descripción
    /// - Estado actual
    /// - Fechas de creación, actualización y vencimiento
    /// - Información del estado asociado
    /// </remarks>
    /// <response code="200">Tarea encontrada exitosamente</response>
    /// <response code="404">Tarea no encontrada con el ID especificado</response>
    /// <response code="401">Token JWT requerido</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskDto), 200)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<TaskDto>> GetTask(int id)
    {
        try
        {
            var task = await _taskService.GetByIdAsync(id);
            if (task == null)
            {
                return NotFound(new { message = $"Tarea con ID {id} no encontrada" });
            }
            return Ok(task);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Crea una nueva tarea en el sistema
    /// </summary>
    /// <param name="createTaskDto">Información de la nueva tarea a crear</param>
    /// <returns>Tarea creada con su ID asignado y información completa</returns>
    /// <remarks>
    /// Ejemplo de uso:
    /// 
    ///     POST /api/Tasks
    ///     {
    ///         "title": "Implementar nueva funcionalidad",
    ///         "description": "Desarrollar el módulo de reportes para el dashboard",
    ///         "stateId": 1,
    ///         "dueDate": "2024-12-31T23:59:59"
    ///     }
    /// 
    /// Campos requeridos:
    /// - **title**: Título descriptivo de la tarea (no puede estar vacío)
    /// - **stateId**: ID válido de un estado existente (mayor a 0)
    /// 
    /// Campos opcionales:
    /// - **description**: Descripción detallada de la tarea
    /// - **dueDate**: Fecha y hora de vencimiento (formato ISO 8601)
    /// 
    /// La tarea se crea automáticamente con:
    /// - Fecha de creación actual
    /// - Fecha de actualización actual
    /// - ID único asignado por el sistema
    /// </remarks>
    /// <response code="201">Tarea creada exitosamente</response>
    /// <response code="400">Datos de entrada inválidos o estado no existe</response>
    /// <response code="401">Token JWT requerido</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), 201)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto createTaskDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(createTaskDto.Title))
            {
                return BadRequest(new { message = "El título de la tarea es requerido" });
            }

            if (createTaskDto.StateId <= 0)
            {
                return BadRequest(new { message = "El ID del estado es requerido y debe ser mayor a 0" });
            }

            var createdTask = await _taskService.CreateAsync(createTaskDto);
            return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza una tarea existente en el sistema
    /// </summary>
    /// <param name="id">ID de la tarea a actualizar</param>
    /// <param name="updateTaskDto">Nuevos datos para la tarea</param>
    /// <returns>Tarea actualizada con la nueva información</returns>
    /// <remarks>
    /// Ejemplo de uso:
    /// 
    ///     PUT /api/Tasks/123
    ///     {
    ///         "title": "Implementar nueva funcionalidad - ACTUALIZADA",
    ///         "description": "Desarrollar el módulo de reportes con gráficos avanzados",
    ///         "stateId": 2,
    ///         "dueDate": "2025-01-15T18:00:00"
    ///     }
    /// 
    /// Comportamiento:
    /// - Solo se actualizan los campos proporcionados
    /// - La fecha de actualización se establece automáticamente
    /// - Se valida que el nuevo estado exista
    /// - Se mantiene la fecha de creación original
    /// 
    /// Casos de uso comunes:
    /// - Cambiar el estado de una tarea (ej: de "Pendiente" a "En Progreso")
    /// - Actualizar la fecha de vencimiento
    /// - Modificar título o descripción
    /// - Reasignar a un estado diferente
    /// </remarks>
    /// <response code="200">Tarea actualizada exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="404">Tarea no encontrada</response>
    /// <response code="401">Token JWT requerido</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TaskDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<TaskDto>> UpdateTask(int id, [FromBody] UpdateTaskDto updateTaskDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(updateTaskDto.Title))
            {
                return BadRequest(new { message = "El título de la tarea es requerido" });
            }

            if (updateTaskDto.StateId <= 0)
            {
                return BadRequest(new { message = "El ID del estado es requerido y debe ser mayor a 0" });
            }

            var updatedTask = await _taskService.UpdateAsync(id, updateTaskDto);
            return Ok(updatedTask);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Elimina una tarea del sistema de forma permanente
    /// </summary>
    /// <param name="id">ID de la tarea a eliminar</param>
    /// <returns>Confirmación de eliminación exitosa</returns>
    /// <remarks>
    /// ⚠️ **ADVERTENCIA**: Esta operación es **IRREVERSIBLE**.
    /// 
    /// Ejemplo de uso: DELETE /api/Tasks/123
    /// 
    /// Consideraciones importantes:
    /// - La tarea se elimina permanentemente de la base de datos
    /// - No se puede recuperar una vez eliminada
    /// - Se recomienda cambiar el estado a "Cancelada" en lugar de eliminar
    /// - Útil para limpiar tareas de prueba o datos erróneos
    /// 
    /// Alternativas recomendadas:
    /// - Cambiar estado a "Cancelada" o "Archivada"
    /// - Implementar eliminación lógica (soft delete)
    /// </remarks>
    /// <response code="204">Tarea eliminada exitosamente</response>
    /// <response code="404">Tarea no encontrada</response>
    /// <response code="401">Token JWT requerido</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> DeleteTask(int id)
    {
        try
        {
            var result = await _taskService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"Tarea con ID {id} no encontrada" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene todos los estados disponibles para asignar a las tareas
    /// </summary>
    /// <returns>Lista completa de estados que se pueden asignar a las tareas</returns>
    /// <remarks>
    /// Este endpoint es útil para:
    /// - Poblar dropdowns/selects en interfaces de usuario
    /// - Validar estados antes de crear/actualizar tareas
    /// - Mostrar opciones disponibles al usuario
    /// 
    /// Ejemplo de uso: GET /api/Tasks/states
    /// 
    /// Típicamente retorna estados como:
    /// - Pendiente
    /// - En Progreso
    /// - En Revisión
    /// - Completada
    /// - Cancelada
    /// 
    /// **Nota**: Este endpoint duplica funcionalidad con /api/States, 
    /// se mantiene por conveniencia y compatibilidad.
    /// </remarks>
    /// <response code="200">Lista de estados obtenida exitosamente</response>
    /// <response code="401">Token JWT requerido</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("states")]
    [ProducesResponseType(typeof(IEnumerable<StateDto>), 200)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<IEnumerable<StateDto>>> GetStates()
    {
        try
        {
            var states = await _taskService.GetStatesAsync();
            return Ok(states);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", details = ex.Message });
        }
    }
}