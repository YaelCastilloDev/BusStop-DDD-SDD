using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusStop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRouteCascadeForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_stops_RouteId",
                table: "stops",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_comments_RouteId",
                table: "comments",
                column: "RouteId");

            migrationBuilder.AddForeignKey(
                name: "fk_comments_route",
                table: "comments",
                column: "RouteId",
                principalTable: "routes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_stops_route",
                table: "stops",
                column: "RouteId",
                principalTable: "routes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_comments_route",
                table: "comments");

            migrationBuilder.DropForeignKey(
                name: "fk_stops_route",
                table: "stops");

            migrationBuilder.DropIndex(
                name: "IX_comments_RouteId",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "IX_stops_RouteId",
                table: "stops");
        }
    }
}
