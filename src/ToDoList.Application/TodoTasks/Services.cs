using ToDoList.Domain.Entities;
using ToDoList.Domain.Repositories;

namespace ToDoList.Application.TodoTasks;

public sealed class CreateTodoTaskService
{
    private readonly ITodoTaskRepository _repository;

    public CreateTodoTaskService(ITodoTaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateTodoTaskResponse> HandleAsync(CreateTodoTaskRequest request, string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Title is required.", nameof(request.Title));
        }

        var now = DateTime.UtcNow;
        var entity = new TodoTask(Guid.NewGuid(), request.Title, request.Description, userId, now);

        await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);

        return new CreateTodoTaskResponse { Id = entity.Id };
    }
}

public sealed class GetTodoTaskService
{
    private readonly ITodoTaskRepository _repository;

    public GetTodoTaskService(ITodoTaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<TodoTaskDto?> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return entity?.ToDto();
    }
}

public sealed class ListTodoTasksService
{
    private readonly ITodoTaskRepository _repository;

    public ListTodoTasksService(ITodoTaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<TodoTaskDto>> HandleAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page <= 0) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize <= 0 || pageSize > 100) throw new ArgumentOutOfRangeException(nameof(pageSize));

        var entities = await _repository.GetPageAsync(page, pageSize, cancellationToken).ConfigureAwait(false);
        var items = entities.Select(e => e.ToDto()).ToArray();

        return new PagedResult<TodoTaskDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize
        };
    }
}

public sealed class UpdateTodoTaskService
{
    private readonly ITodoTaskRepository _repository;

    public UpdateTodoTaskService(ITodoTaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> HandleAsync(Guid id, UpdateTodoTaskRequest request, string userId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Title is required.", nameof(request.Title));
        }

        entity.UpdateDetails(request.Title, request.Description, userId, DateTime.UtcNow);

        if (request.IsCompleted && !entity.IsCompleted)
        {
            entity.MarkCompleted(userId, DateTime.UtcNow);
        }

        await _repository.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        return true;
    }
}

public sealed class DeleteTodoTaskService
{
    private readonly ITodoTaskRepository _repository;

    public DeleteTodoTaskService(ITodoTaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> HandleAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            return false;
        }

        entity.SoftDelete(userId, DateTime.UtcNow);
        await _repository.SoftDeleteAsync(entity, cancellationToken).ConfigureAwait(false);
        return true;
    }
}

