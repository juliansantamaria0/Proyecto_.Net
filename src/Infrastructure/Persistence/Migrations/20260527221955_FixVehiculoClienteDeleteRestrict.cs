using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoTallerManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixVehiculoClienteDeleteRestrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehiculos_Clientes_ClienteId",
                table: "Vehiculos");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehiculos_Clientes_ClienteId",
                table: "Vehiculos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehiculos_Clientes_ClienteId",
                table: "Vehiculos");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehiculos_Clientes_ClienteId",
                table: "Vehiculos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
