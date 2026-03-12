using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoList.Infrastructure.Data.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "todo_tasks",
            columns: table => new
            {
                id = table.Column<Guid>(type: "char(36)", nullable: false)
                    .Annotation("MySql:CharSet", "ascii_general_ci"),
                title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                is_completed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                created_by = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                updated_by = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                row_version = table.Column<byte[]>(type: "timestamp(6)", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_todo_tasks", x => x.id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "ix_todo_tasks_is_completed",
            table: "todo_tasks",
            column: "is_completed");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "todo_tasks");
    }
}

