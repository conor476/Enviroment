using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enviroment.Migrations
{
    /// <inheritdoc />
    public partial class initialcreate39 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryID",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "EmployeeID",
                table: "Tickets",
                newName: "CustomerID");

            migrationBuilder.AddColumn<string>(
                name: "EmployeeName",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Team",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeName",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Team",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "CustomerID",
                table: "Tickets",
                newName: "EmployeeID");

            migrationBuilder.AddColumn<int>(
                name: "CategoryID",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
