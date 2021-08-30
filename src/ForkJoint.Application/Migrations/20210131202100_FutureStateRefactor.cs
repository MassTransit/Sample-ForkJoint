using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ForkJoint.Application.Migrations
{
    public partial class FutureStateRefactor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FutureState",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(nullable: false),
                    CurrentState = table.Column<int>(nullable: false),
                    RetryAttempt = table.Column<int>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Deadline = table.Column<DateTime>(nullable: true),
                    Completed = table.Column<DateTime>(nullable: true),
                    Canceled = table.Column<DateTime>(nullable: true),
                    Faulted = table.Column<DateTime>(nullable: true),
                    Location = table.Column<string>(nullable: true),
                    Request = table.Column<string>(nullable: true),
                    Pending = table.Column<string>(nullable: true),
                    Subscriptions = table.Column<string>(nullable: true),
                    Variables = table.Column<string>(nullable: true),
                    Results = table.Column<string>(nullable: true),
                    Faults = table.Column<string>(nullable: true),
                    Version = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FutureState", x => x.CorrelationId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FutureState");
        }
    }
}
