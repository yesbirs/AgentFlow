using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentFlow.API.Migrations
{
    /// <inheritdoc />
    public partial class AdduserInWorkFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "WorkflowDefinitions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "WorkflowDefinitions");
        }
    }
}
