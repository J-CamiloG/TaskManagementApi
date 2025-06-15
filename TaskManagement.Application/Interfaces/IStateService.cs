using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Interfaces;

public interface IStateService
{
    Task<IEnumerable<StateDto>> GetAllAsync();
    Task<StateDto?> GetByIdAsync(int id);
    Task<StateDto> CreateAsync(CreateStateDto createStateDto);
    Task<StateDto> UpdateAsync(int id, UpdateStateDto updateStateDto);
    Task<bool> DeleteAsync(int id);
}