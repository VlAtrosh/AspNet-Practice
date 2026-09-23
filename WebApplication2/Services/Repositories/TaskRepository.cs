using Microsoft.EntityFrameworkCore;
using TaskScheduler.MinimalAPI.Data;
using TaskScheduler.MinimalAPI.Models.Entities;
using WebApplication1.Data;

namespace TaskScheduler.MinimalAPI.Services.Repositories;

/// <summary>
/// Реализация репозитория задач с EF Core
/// </summary>
public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskEntity>> GetAllAsync(bool includeDeleted = false)
    {
        var query = _context.Tasks.AsQueryable();
        if (!includeDeleted)
            query = query.Where(t => t.DeletedAt == null);
        return await query.ToListAsync();
    }

    public async Task<TaskEntity?> GetByIdAsync(Guid id, bool includeDeleted = false)
    {
        var query = _context.Tasks.AsQueryable();
        if (!includeDeleted)
            query = query.Where(t => t.DeletedAt == null);
        return await query.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<TaskEntity>> GetByCategoryAsync(string category)
    {
        return await _context.Tasks
            .Where(t => t.Category == category && t.DeletedAt == null)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskEntity>> GetByStatusAsync(string status)
    {
        return await _context.Tasks
            .Where(t => t.Status == status && t.DeletedAt == null)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskEntity>> GetOverdueAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Tasks
            .Where(t => t.DueDate < now
                        && t.Status != "Done"
                        && t.Status != "Cancelled"
                        && t.DeletedAt == null)
            .ToListAsync();
    }

    public async Task<TaskEntity> CreateAsync(TaskEntity entity)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        await _context.Tasks.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<TaskEntity> UpdateAsync(TaskEntity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Tasks.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return false;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Tasks
            .AnyAsync(t => t.Id == id && t.DeletedAt == null);
    }

    public async Task<int> CountAsync()
    {
        return await _context.Tasks.CountAsync(t => t.DeletedAt == null);
    }
}