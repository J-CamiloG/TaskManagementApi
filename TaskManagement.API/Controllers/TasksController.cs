using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Listar tareas con paginación y filtros
    /// </summary>
    [HttpGet]
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
    /// Obtener tarea específica con detalles
    /// </summary>
    [HttpGet("{id}")]
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
    /// Crear nueva tarea
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateTask(CreateTaskDto createTaskDto)
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
    /// Actualizar tarea existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TaskDto>> UpdateTask(int id, UpdateTaskDto updateTaskDto)
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
    /// Eliminar tarea
    /// </summary>
    [HttpDelete("{id}")]
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
    /// Listar estados disponibles
    /// </summary>
    [HttpGet("states")]
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