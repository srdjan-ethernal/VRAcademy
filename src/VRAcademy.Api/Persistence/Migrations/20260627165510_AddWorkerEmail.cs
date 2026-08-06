using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VRAcademy.Api.Persistence.Migrations
{
    public partial class AddWorkerEmail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Workers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Workers");
        }
    }
}
