using ToDoList.Domain.Entities;

namespace ToDoList.Application.TodoTasks;

public sealed class TodoTaskDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed class CreateTodoTaskRequest
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public sealed class CreateTodoTaskResponse
{
    public Guid Id { get; init; }
}

public sealed class UpdateTodoTaskRequest
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsCompleted { get; init; }
}

public sealed class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}

internal static class TodoTaskMappingExtensions
{
    public static TodoTaskDto ToDto(this TodoTask entity)
    {
        return new TodoTaskDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            IsCompleted = entity.IsCompleted,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}

