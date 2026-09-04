using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeuroSync.Migrations
{
    /// <inheritdoc />
    public partial class AdicionandoResponsavel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "data_cadastro",
                table: "paciente",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "responsavel",
                table: "paciente",
                type: "TEXT",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "data_cadastro",
                table: "paciente");

            migrationBuilder.DropColumn(
                name: "responsavel",
                table: "paciente");
        }
    }
}
