using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enviroment.Migrations
{
    /// <inheritdoc />
    public partial class Migration108 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "About",
                table: "Categorys",
                type: "nvarchar(2550)",
                maxLength: 2550,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Group",
                table: "Categorys",
                type: "nvarchar(2550)",
                maxLength: 2550,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "About",
                table: "Categorys");

            migrationBuilder.DropColumn(
                name: "Group",
                table: "Categorys");
        }
    }
}
