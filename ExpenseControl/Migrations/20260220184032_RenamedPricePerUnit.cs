using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseControl.Migrations
{
    /// <inheritdoc />
    public partial class RenamedPricePerUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PricePerUnit",
                table: "Items",
                newName: "UnitPrice");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "Items",
                newName: "PricePerUnit");
        }
    }
}
