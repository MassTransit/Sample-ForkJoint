using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForkJoint.Api.Migrations
{
    public partial class ef6update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "FutureState",
                type: "varbinary(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "FutureState");
        }
    }
}
