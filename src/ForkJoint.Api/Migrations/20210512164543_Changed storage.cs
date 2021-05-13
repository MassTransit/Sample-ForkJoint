using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ForkJoint.Api.Migrations
{
    public partial class Changedstorage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Canceled",
                table: "FutureState");

            migrationBuilder.DropColumn(
                name: "Deadline",
                table: "FutureState");

            migrationBuilder.DropColumn(
                name: "Request",
                table: "FutureState");

            migrationBuilder.DropColumn(
                name: "RetryAttempt",
                table: "FutureState");

            migrationBuilder.AddColumn<string>(
                name: "Command",
                table: "FutureState",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Command",
                table: "FutureState");

            migrationBuilder.AddColumn<DateTime>(
                name: "Canceled",
                table: "FutureState",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Deadline",
                table: "FutureState",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Request",
                table: "FutureState",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryAttempt",
                table: "FutureState",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
