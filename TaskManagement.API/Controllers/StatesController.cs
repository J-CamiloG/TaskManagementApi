using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatesController : ControllerBase
{
    private readonly IStateService _stateService;

    public StatesController(IStateService stateService)
    {
        _stateService = stateService;
    }

    /// <summary>
    /// Obtener lista de estados
    /// </summary>
    [HttpGet]
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
    /// Obtener estado específico
    /// </summary>
    [HttpGet("{id}")]
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
    /// Crear nuevo estado
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<StateDto>> CreateState(CreateStateDto createStateDto)
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
    /// Actualizar estado
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<StateDto>> UpdateState(int id, UpdateStateDto updateStateDto)
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
    /// Eliminar estado
    /// </summary>
    [HttpDelete("{id}")]
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