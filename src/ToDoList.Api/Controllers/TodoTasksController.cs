using Microsoft.AspNetCore.Mvc;
using ToDoList.Application.TodoTasks;

namespace ToDoList.Api.Controllers;

[ApiController]
[Route("api/v1/todos")]
public class TodoTasksController : ControllerBase
{
    private readonly CreateTodoTaskService _createService;
    private readonly GetTodoTaskService _getService;
    private readonly ListTodoTasksService _listService;
    private readonly UpdateTodoTaskService _updateService;
    private readonly DeleteTodoTaskService _deleteService;

    public TodoTasksController(
        CreateTodoTaskService createService,
        GetTodoTaskService getService,
        ListTodoTasksService listService,
        UpdateTodoTaskService updateService,
        DeleteTodoTaskService deleteService)
    {
        _createService = createService;
        _getService = getService;
        _listService = listService;
        _updateService = updateService;
        _deleteService = deleteService;
    }

    /// <summary>
    /// Creates a new to-do task.
    /// </summary>
    /// <remarks>
    /// Use this endpoint to create a new task with a title and optional description.
    /// </remarks>
    /// <param name="request">The task details.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The identifier of the created task.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateTodoTaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateTodoTaskResponse>> CreateAsync([FromBody] CreateTodoTaskRequest request, CancellationToken cancellationToken)
    {
        var userId = "system"; // placeholder for future auth

        var result = await _createService.HandleAsync(request, userId, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TodoTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodoTaskDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await _getService.HandleAsync(id, cancellationToken);
        if (task is null)
        {
            return NotFound();
        }

        return Ok(task);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TodoTaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TodoTaskDto>>> GetPageAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _listService.HandleAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateTodoTaskRequest request, CancellationToken cancellationToken)
    {
        var userId = "system"; // placeholder for future auth

        var updated = await _updateService.HandleAsync(id, request, userId, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = "system"; // placeholder for future auth

        var deleted = await _deleteService.HandleAsync(id, userId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}

