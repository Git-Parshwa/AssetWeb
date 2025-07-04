﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyImageColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyImageUrl",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyImageUrl",
                table: "Companies");
        }
    }
}
