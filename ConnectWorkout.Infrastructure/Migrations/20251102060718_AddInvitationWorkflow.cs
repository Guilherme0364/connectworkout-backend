using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConnectWorkout.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitationWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "ConnectedAt",
                table: "StudentInstructors",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "InvitedAt",
                table: "StudentInstructors",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "RespondedAt",
                table: "StudentInstructors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "StudentInstructors",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvitedAt",
                table: "StudentInstructors");

            migrationBuilder.DropColumn(
                name: "RespondedAt",
                table: "StudentInstructors");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "StudentInstructors");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ConnectedAt",
                table: "StudentInstructors",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
