using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusStop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRouteModeratedColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ModeratedAt",
                table: "routes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ModeratedBy",
                table: "routes",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_moderation_actions_IssuedBy",
                table: "moderation_actions",
                column: "IssuedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_moderation_actions_IssuedBy",
                table: "moderation_actions");

            migrationBuilder.DropColumn(
                name: "ModeratedAt",
                table: "routes");

            migrationBuilder.DropColumn(
                name: "ModeratedBy",
                table: "routes");
        }
    }
}
