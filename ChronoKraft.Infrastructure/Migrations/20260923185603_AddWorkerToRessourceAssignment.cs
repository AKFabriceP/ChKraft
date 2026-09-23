using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChronoKraft.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkerToRessourceAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkerId",
                table: "ResourceAssignments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceAssignments_WorkerId",
                table: "ResourceAssignments",
                column: "WorkerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResourceAssignments_Workers_WorkerId",
                table: "ResourceAssignments",
                column: "WorkerId",
                principalTable: "Workers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResourceAssignments_Workers_WorkerId",
                table: "ResourceAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ResourceAssignments_WorkerId",
                table: "ResourceAssignments");

            migrationBuilder.DropColumn(
                name: "WorkerId",
                table: "ResourceAssignments");
        }
    }
}
