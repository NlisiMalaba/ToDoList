using Microsoft.EntityFrameworkCore;
using ToDoList.Domain.Entities;
using ToDoList.Domain.Repositories;
using ToDoList.Infrastructure.Data;

namespace ToDoList.Infrastructure.Repositories;

public class TodoTaskRepository : ITodoTaskRepository
{
    private readonly AppDbContext _dbContext;

    public TodoTaskRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TodoTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TodoTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<TodoTask>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TodoTasks
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task AddAsync(TodoTask task, CancellationToken cancellationToken = default)
    {
        await _dbContext.TodoTasks.AddAsync(task, cancellationToken).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(TodoTask task, CancellationToken cancellationToken = default)
    {
        _dbContext.TodoTasks.Update(task);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task SoftDeleteAsync(TodoTask task, CancellationToken cancellationToken = default)
    {
        _dbContext.TodoTasks.Update(task);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

