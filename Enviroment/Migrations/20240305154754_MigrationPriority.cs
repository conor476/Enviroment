using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enviroment.Migrations
{
    /// <inheritdoc />
    public partial class MigrationPriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PriorityLevel",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriorityLevel",
                table: "Tickets");
        }
    }
}
