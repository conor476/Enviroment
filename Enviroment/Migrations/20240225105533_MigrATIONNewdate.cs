﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enviroment.Migrations
{
    /// <inheritdoc />
    public partial class MigrATIONNewdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "Tickets",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "Tickets");
        }
    }
}
