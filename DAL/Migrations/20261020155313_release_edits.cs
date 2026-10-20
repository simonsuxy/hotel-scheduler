using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class release_edits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ChargeCategories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ChargeCategories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ChargeCategories",
                keyColumn: "Id",
                keyValue: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ChargeCategories",
                columns: new[] { "Id", "DefaultPricePerNight", "Description", "IsPreCreated", "Name", "Type" },
                values: new object[,]
                {
                    { 1, 20m, "Charge per adult guest", true, "Adults", 0 },
                    { 2, 10m, "Charge per child guest", true, "Kids", 0 },
                    { 3, 15m, "Charge per pet", true, "Pets", 0 }
                });
        }
    }
}
