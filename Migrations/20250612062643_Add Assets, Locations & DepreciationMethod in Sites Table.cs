using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetsLocationsDepreciationMethodinSitesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Assets",
                table: "Sites",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DepreciationMethod",
                table: "Sites",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Locations",
                table: "Sites",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Assets",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "DepreciationMethod",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "Locations",
                table: "Sites");
        }
    }
}
