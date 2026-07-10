using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FactoryQueueSystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class VehicleAssignmentFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Vehicles_VehicleId",
                table: "Shipments");

            migrationBuilder.AlterColumn<Guid>(
                name: "VehicleId",
                table: "Shipments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Shipments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE s
                SET s.UserId = v.UserId
                FROM Shipments s
                INNER JOIN Vehicles v ON s.VehicleId = v.Id
                WHERE s.UserId IS NULL
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_UserId",
                table: "Shipments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Users_UserId",
                table: "Shipments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Vehicles_VehicleId",
                table: "Shipments",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Users_UserId",
                table: "Shipments");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Vehicles_VehicleId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_UserId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Shipments");

            migrationBuilder.AlterColumn<Guid>(
                name: "VehicleId",
                table: "Shipments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Vehicles_VehicleId",
                table: "Shipments",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
