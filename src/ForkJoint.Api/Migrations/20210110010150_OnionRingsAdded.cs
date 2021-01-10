using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ForkJoint.Api.Migrations
{
    public partial class OnionRingsAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OnionRingsState",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(nullable: false),
                    CurrentState = table.Column<int>(nullable: false),
                    ResponseAddress = table.Column<string>(nullable: true),
                    RequestId = table.Column<Guid>(nullable: true),
                    Created = table.Column<DateTime>(nullable: true),
                    Completed = table.Column<DateTime>(nullable: true),
                    Faulted = table.Column<DateTime>(nullable: true),
                    OrderId = table.Column<Guid>(nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    ExceptionInfo = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnionRingsState", x => x.CorrelationId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OnionRingsState");
        }
    }
}
