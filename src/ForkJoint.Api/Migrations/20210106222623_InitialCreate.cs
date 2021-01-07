using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ForkJoint.Api.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BurgerState",
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
                    Burger = table.Column<string>(nullable: true),
                    TrackingNumber = table.Column<Guid>(nullable: false),
                    ExceptionInfo = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BurgerState", x => x.CorrelationId);
                });

            migrationBuilder.CreateTable(
                name: "OrderState",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(nullable: false),
                    CurrentState = table.Column<int>(nullable: false),
                    ResponseAddress = table.Column<string>(nullable: true),
                    RequestId = table.Column<Guid>(nullable: true),
                    Created = table.Column<DateTime>(nullable: true),
                    Completed = table.Column<DateTime>(nullable: true),
                    Faulted = table.Column<DateTime>(nullable: true),
                    Updated = table.Column<DateTime>(nullable: true),
                    LineCount = table.Column<int>(nullable: false),
                    LinesPending = table.Column<string>(nullable: true),
                    LinesCompleted = table.Column<string>(nullable: true),
                    LinesFaulted = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderState", x => x.CorrelationId);
                });

            migrationBuilder.CreateTable(
                name: "RequestState",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(nullable: false),
                    CurrentState = table.Column<int>(nullable: false),
                    ConversationId = table.Column<Guid>(nullable: true),
                    ResponseAddress = table.Column<string>(nullable: true),
                    FaultAddress = table.Column<string>(nullable: true),
                    ExpirationTime = table.Column<DateTime>(nullable: true),
                    SagaCorrelationId = table.Column<Guid>(nullable: false),
                    SagaAddress = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestState", x => x.CorrelationId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestState_SagaCorrelationId",
                table: "RequestState",
                column: "SagaCorrelationId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BurgerState");

            migrationBuilder.DropTable(
                name: "OrderState");

            migrationBuilder.DropTable(
                name: "RequestState");
        }
    }
}
