using ToDoList.Domain.Entities;

namespace ToDoList.Domain.Repositories;

public interface ITodoTaskRepository
{
    Task<TodoTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TodoTask>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task AddAsync(TodoTask task, CancellationToken cancellationToken = default);

    Task UpdateAsync(TodoTask task, CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(TodoTask task, CancellationToken cancellationToken = default);
}

