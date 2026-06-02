using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentFlow.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdInProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Projects",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Projects");
        }
    }
}
