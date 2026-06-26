using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusStop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameDeleteCommentToModerate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "comments",
                newName: "ModeratedAt");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "comments",
                newName: "ModeratedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ModeratedAt",
                table: "comments",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "ModeratedBy",
                table: "comments",
                newName: "DeletedBy");
        }
    }
}