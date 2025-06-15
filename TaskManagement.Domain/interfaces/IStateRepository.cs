using TaskManagement.Domain.Entities;

namespace TaskManagement.Domain.Interfaces;

public interface IStateRepository
{
    Task<IEnumerable<State>> GetAllAsync();
    Task<State?> GetByIdAsync(int id);
    Task<State> CreateAsync(State state);
    Task<State> UpdateAsync(State state);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(string name);
    Task<bool> HasTasksAsync(int stateId);
}