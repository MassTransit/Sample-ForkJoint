using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ForkJoint.Application.Migrations
{
    public partial class optimisticconcurrencytests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OptimisticConcurrencyTestsState",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(nullable: false),
                    CurrentState = table.Column<int>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Completed = table.Column<DateTime>(nullable: true),
                    Faulted = table.Column<DateTime>(nullable: true),
                    Version = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptimisticConcurrencyTestsState", x => x.CorrelationId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OptimisticConcurrencyTestsState");
        }
    }
}
