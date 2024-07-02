using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blazor.Migrations
{
    public partial class Reservations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add the Floor column to the Reservations table
            migrationBuilder.AddColumn<int>(
                name: "Floor",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkspaceId",
                table: "Reservations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_WorkspaceId",
                table: "Reservations",
                column: "WorkspaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Workspaces_WorkspaceId",
                table: "Reservations",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Workspaces_WorkspaceId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_WorkspaceId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Reservations");

            // Remove the Floor column from the Reservations table
            migrationBuilder.DropColumn(
                name: "Floor",
                table: "Reservations");
        }
    }
}