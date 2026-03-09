using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using ToDoList.Application.TodoTasks;
using ToDoList.Domain.Entities;
using ToDoList.Domain.Repositories;

namespace ToDoList.UnitTests;

public sealed class CreateTodoTaskServiceTests
{
    private sealed class InMemoryTodoTaskRepository : ITodoTaskRepository
    {
        public TodoTask? LastAdded { get; private set; }

        public Task AddAsync(TodoTask entity, CancellationToken cancellationToken = default)
        {
            LastAdded = entity;
            return Task.CompletedTask;
        }

        public Task<TodoTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<TodoTask?>(LastAdded?.Id == id ? LastAdded : null);

        public Task<IReadOnlyList<TodoTask>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<TodoTask>>(Array.Empty<TodoTask>());

        public Task UpdateAsync(TodoTask entity, CancellationToken cancellationToken = default)
        {
            LastAdded = entity;
            return Task.CompletedTask;
        }

        public Task SoftDeleteAsync(TodoTask task, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Title_Is_Empty()
    {
        var repository = new InMemoryTodoTaskRepository();
        var service = new CreateTodoTaskService(repository);
        var request = new CreateTodoTaskRequest { Title = string.Empty, Description = "desc" };

        var act = async () => await service.HandleAsync(request, "user-1", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Title is required*")
            .Where(e => e.ParamName == "Title");
    }

    [Fact]
    public async Task HandleAsync_Persists_Task_When_Request_Is_Valid()
    {
        var repository = new InMemoryTodoTaskRepository();
        var service = new CreateTodoTaskService(repository);
        var request = new CreateTodoTaskRequest { Title = "Buy milk", Description = "Whole milk" };

        var response = await service.HandleAsync(request, "user-1", CancellationToken.None);

        response.Id.Should().NotBeEmpty();
        repository.LastAdded.Should().NotBeNull();
        repository.LastAdded!.Title.Should().Be("Buy milk");
        repository.LastAdded.Description.Should().Be("Whole milk");
    }
}

