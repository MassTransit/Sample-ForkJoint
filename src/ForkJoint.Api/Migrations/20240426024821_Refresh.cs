using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForkJoint.Api.Migrations
{
    /// <inheritdoc />
    public partial class Refresh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FutureState",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentState = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Completed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Faulted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Command = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pending = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subscriptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Variables = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Results = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Faults = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FutureState", x => x.CorrelationId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FutureState");
        }
    }
}
