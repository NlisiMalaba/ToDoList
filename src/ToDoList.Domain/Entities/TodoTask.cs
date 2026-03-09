using System;

namespace ToDoList.Domain.Entities;

public class TodoTask
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public bool IsCompleted { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = "system";
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private TodoTask()
    {
        // For EF Core
    }

    public TodoTask(Guid id, string title, string? description, string createdBy, DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        Id = id;
        Title = title.Trim();
        Description = description;
        CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "system" : createdBy;
        CreatedAt = DateTime.SpecifyKind(createdAtUtc, DateTimeKind.Utc);
        IsCompleted = false;
        IsDeleted = false;
    }

    public void UpdateDetails(string title, string? description, string updatedBy, DateTime updatedAtUtc)
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Cannot update a deleted task.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        Title = title.Trim();
        Description = description;
        UpdatedBy = string.IsNullOrWhiteSpace(updatedBy) ? "system" : updatedBy;
        UpdatedAt = DateTime.SpecifyKind(updatedAtUtc, DateTimeKind.Utc);
    }

    public void MarkCompleted(string updatedBy, DateTime updatedAtUtc)
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Cannot complete a deleted task.");
        }

        IsCompleted = true;
        UpdatedBy = string.IsNullOrWhiteSpace(updatedBy) ? "system" : updatedBy;
        UpdatedAt = DateTime.SpecifyKind(updatedAtUtc, DateTimeKind.Utc);
    }

    public void SoftDelete(string deletedBy, DateTime deletedAtUtc)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        UpdatedBy = string.IsNullOrWhiteSpace(deletedBy) ? "system" : deletedBy;
        UpdatedAt = DateTime.SpecifyKind(deletedAtUtc, DateTimeKind.Utc);
    }
}

