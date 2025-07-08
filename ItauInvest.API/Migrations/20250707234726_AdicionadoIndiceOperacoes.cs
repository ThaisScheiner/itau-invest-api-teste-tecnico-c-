using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ItauInvest.API.Migrations
{
    /// <inheritdoc />
    public partial class AdicionadoIndiceOperacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Operacoes_Usuario_Ativo_Data",
                table: "operacoes",
                columns: new[] { "UsuarioId", "AtivoId", "DataHora" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Operacoes_Usuario_Ativo_Data",
                table: "operacoes");
        }
    }
}
