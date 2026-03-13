using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToDoList.Domain.Entities;

namespace ToDoList.Infrastructure.Data.Config;

public class TodoTaskConfiguration : IEntityTypeConfiguration<TodoTask>
{
    public void Configure(EntityTypeBuilder<TodoTask> builder)
    {
        builder.ToTable("todo_tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(t => t.Title)
            .HasColumnName("title")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(t => t.IsCompleted)
            .HasColumnName("is_completed")
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(t => t.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        builder.Property(t => t.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired();

        builder.Property(t => t.RowVersion)
            .HasColumnName("row_version")
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        builder.HasIndex(t => t.IsCompleted).HasDatabaseName("ix_todo_tasks_is_completed");

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}

