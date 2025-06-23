using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cb5d3c32-df7d-4688-8ac0-04bf69471c80",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "User", "USER" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cb5d3c32-df7d-4688-8ac0-04bf69471c80",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "Comapany", "COMPANY" });
        }
    }
}
