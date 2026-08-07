using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VRAcademy.Api.Persistence.Migrations;

[DbContext(typeof(TrainingDbContext))]
[Migration("20260805090000_AddEnrollmentDueAt")]
public partial class AddEnrollmentDueAt : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "DueAt",
            table: "Enrollments",
            type: "datetimeoffset",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DueAt",
            table: "Enrollments");
    }
}
