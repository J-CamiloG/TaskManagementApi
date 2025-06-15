using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories;

public class StateRepository : IStateRepository
{
    private readonly TaskManagementDbContext _context;

    public StateRepository(TaskManagementDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<State>> GetAllAsync()
    {
        return await _context.States
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<State?> GetByIdAsync(int id)
    {
        return await _context.States
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<State> CreateAsync(State state)
    {
        state.CreatedAt = DateTime.UtcNow;
        state.UpdatedAt = DateTime.UtcNow;
        
        _context.States.Add(state);
        await _context.SaveChangesAsync();
        return state;
    }

    public async Task<State> UpdateAsync(State state)
    {
        state.UpdatedAt = DateTime.UtcNow;
        
        _context.States.Update(state);
        await _context.SaveChangesAsync();
        return state;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var state = await _context.States.FindAsync(id);
        if (state == null) return false;

        _context.States.Remove(state);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(string name)
    {
        return await _context.States
            .AnyAsync(s => s.Name.ToLower() == name.ToLower());
    }

    public async Task<bool> HasTasksAsync(int stateId)
    {
        return await _context.Tasks
            .AnyAsync(t => t.StateId == stateId);
    }
}