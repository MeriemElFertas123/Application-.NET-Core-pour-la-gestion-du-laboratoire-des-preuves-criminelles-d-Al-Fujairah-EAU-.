using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class affModiffinaaaaaaal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Affaires_Enqueteurs_IdEnqueteurResponsable",
                table: "Affaires");

            migrationBuilder.DropIndex(
                name: "IX_Affaires_IdEnqueteurResponsable",
                table: "Affaires");

            migrationBuilder.DropColumn(
                name: "EstArchivee",
                table: "Affaires");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Affaires",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroAffaire",
                table: "Affaires",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Lieu",
                table: "Affaires",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "CreeParUserId",
                table: "Affaires",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreation",
                table: "Affaires",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateModification",
                table: "Affaires",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ModifieParUserId",
                table: "Affaires",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotifsRecurrentsJson",
                table: "Affaires",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomEnqueteur",
                table: "Affaires",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreeParUserId",
                table: "Affaires");

            migrationBuilder.DropColumn(
                name: "DateCreation",
                table: "Affaires");

            migrationBuilder.DropColumn(
                name: "DateModification",
                table: "Affaires");

            migrationBuilder.DropColumn(
                name: "ModifieParUserId",
                table: "Affaires");

            migrationBuilder.DropColumn(
                name: "MotifsRecurrentsJson",
                table: "Affaires");

            migrationBuilder.DropColumn(
                name: "NomEnqueteur",
                table: "Affaires");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Affaires",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroAffaire",
                table: "Affaires",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Lieu",
                table: "Affaires",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EstArchivee",
                table: "Affaires",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Affaires_IdEnqueteurResponsable",
                table: "Affaires",
                column: "IdEnqueteurResponsable");

            migrationBuilder.AddForeignKey(
                name: "FK_Affaires_Enqueteurs_IdEnqueteurResponsable",
                table: "Affaires",
                column: "IdEnqueteurResponsable",
                principalTable: "Enqueteurs",
                principalColumn: "idEnqueteur",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
