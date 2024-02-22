using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enviroment.Migrations
{
    /// <inheritdoc />
    public partial class migratioKPI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedDate",
                table: "Tickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OpenedDate",
                table: "Tickets",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosedDate",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "OpenedDate",
                table: "Tickets");
        }
    }
}
