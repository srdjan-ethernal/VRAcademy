using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VRAcademy.Api.Persistence.Migrations;

public partial class AddSubscriptionLevelAndExamId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "SubscriptionLevel",
            table: "Companies",
            type: "nvarchar(40)",
            maxLength: 40,
            nullable: false,
            defaultValue: "SmallBusiness");

        migrationBuilder.AddColumn<string>(
            name: "ExamId",
            table: "Enrollments",
            type: "nvarchar(80)",
            maxLength: 80,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "SubscriptionLevel",
            table: "Companies");

        migrationBuilder.DropColumn(
            name: "ExamId",
            table: "Enrollments");
    }
}
