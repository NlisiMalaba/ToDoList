using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ToDoList.Infrastructure.Data;

#nullable disable

namespace ToDoList.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
partial class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 64);

        modelBuilder
            .HasAnnotation("MySql:CharSet", "utf8mb4");

        modelBuilder.Entity("ToDoList.Domain.Entities.TodoTask", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("char(36)")
                    .HasColumnName("id")
                    .IsRequired()
                    .HasCollation("ascii_general_ci");

                b.Property<string>("CreatedBy")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)")
                    .HasColumnName("created_by")
                    .HasCharSet("utf8mb4");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("datetime(6)")
                    .HasColumnName("created_at");

                b.Property<string>("Description")
                    .HasMaxLength(2000)
                    .HasColumnType("varchar(2000)")
                    .HasColumnName("description")
                    .HasCharSet("utf8mb4");

                b.Property<bool>("IsCompleted")
                    .HasColumnType("tinyint(1)")
                    .HasColumnName("is_completed");

                b.Property<bool>("IsDeleted")
                    .HasColumnType("tinyint(1)")
                    .HasColumnName("is_deleted");

                b.Property<byte[]>("RowVersion")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnType("timestamp(6)")
                    .HasColumnName("row_version");

                b.Property<string>("Title")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("varchar(200)")
                    .HasColumnName("title")
                    .HasCharSet("utf8mb4");

                b.Property<DateTime?>("UpdatedAt")
                    .HasColumnType("datetime(6)")
                    .HasColumnName("updated_at");

                b.Property<string>("UpdatedBy")
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)")
                    .HasColumnName("updated_by")
                    .HasCharSet("utf8mb4");

                b.HasKey("Id");

                b.HasIndex("IsCompleted")
                    .HasDatabaseName("ix_todo_tasks_is_completed");

                b.ToTable("todo_tasks", (string)null);

                b.HasQueryFilter("!IsDeleted");
            });
    }
}

