using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Interfaces;

public interface ITaskService
{
    Task<PagedResultDto<TaskDto>> GetAllAsync(int page = 1, int pageSize = 10, int? stateId = null, DateTime? dueDate = null);
    Task<TaskDto?> GetByIdAsync(int id);
    Task<TaskDto> CreateAsync(CreateTaskDto createTaskDto);
    Task<TaskDto> UpdateAsync(int id, UpdateTaskDto updateTaskDto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<StateDto>> GetStatesAsync();
}