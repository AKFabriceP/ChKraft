using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChronoKraft.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCrossProjectAccessLevelToProjectManager : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CrossProjectAccessLevel",
                table: "ProjectManagers",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CrossProjectAccessLevel",
                table: "ProjectManagers");
        }
    }
}
