using AutoMapper;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IStateRepository _stateRepository;
    private readonly IMapper _mapper;

    public TaskService(ITaskRepository taskRepository, IStateRepository stateRepository, IMapper mapper)
    {
        _taskRepository = taskRepository;
        _stateRepository = stateRepository;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<TaskDto>> GetAllAsync(int page = 1, int pageSize = 10, int? stateId = null, DateTime? dueDate = null)
    {
        var tasks = await _taskRepository.GetAllAsync(page, pageSize, stateId, dueDate);
        var totalCount = await _taskRepository.GetTotalCountAsync(stateId, dueDate);

        return new PagedResultDto<TaskDto>
        {
            Items = _mapper.Map<IEnumerable<TaskDto>>(tasks),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<TaskDto?> GetByIdAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        return task == null ? null : _mapper.Map<TaskDto>(task);
    }

    public async Task<TaskDto> CreateAsync(CreateTaskDto createTaskDto)
    {
        // Validar que el estado existe
        var state = await _stateRepository.GetByIdAsync(createTaskDto.StateId);
        if (state == null)
        {
            throw new KeyNotFoundException($"Estado con ID {createTaskDto.StateId} no encontrado");
        }

        var task = _mapper.Map<TaskItem>(createTaskDto);
        var createdTask = await _taskRepository.CreateAsync(task);
        
        // Cargar el estado para el DTO
        createdTask.State = state;
        return _mapper.Map<TaskDto>(createdTask);
    }

    public async Task<TaskDto> UpdateAsync(int id, UpdateTaskDto updateTaskDto)
    {
        var existingTask = await _taskRepository.GetByIdAsync(id);
        if (existingTask == null)
        {
            throw new KeyNotFoundException($"Tarea con ID {id} no encontrada");
        }

        // Validar que el estado existe
        var state = await _stateRepository.GetByIdAsync(updateTaskDto.StateId);
        if (state == null)
        {
            throw new KeyNotFoundException($"Estado con ID {updateTaskDto.StateId} no encontrado");
        }

        _mapper.Map(updateTaskDto, existingTask);
        var updatedTask = await _taskRepository.UpdateAsync(existingTask);
        
        // Cargar el estado para el DTO
        updatedTask.State = state;
        return _mapper.Map<TaskDto>(updatedTask);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _taskRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<StateDto>> GetStatesAsync()
    {
        var states = await _stateRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<StateDto>>(states);
    }
}