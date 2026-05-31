using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoTallerManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStockMinimoToRepuestos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockMinimo",
                table: "Repuestos",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockMinimo",
                table: "Repuestos");
        }
    }
}
