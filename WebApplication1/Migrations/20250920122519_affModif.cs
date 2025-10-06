using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class affModif : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MotifsRecurrents",
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
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Titre",
                table: "Affaires",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroAffaire",
                table: "Affaires",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Lieu",
                table: "Affaires",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Affaires",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Affaires_Enqueteurs_IdEnqueteurResponsable",
                table: "Affaires");

            migrationBuilder.DropIndex(
                name: "IX_Affaires_IdEnqueteurResponsable",
                table: "Affaires");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Affaires",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "Titre",
                table: "Affaires",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroAffaire",
                table: "Affaires",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Lieu",
                table: "Affaires",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Affaires",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<string>(
                name: "MotifsRecurrents",
                table: "Affaires",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomEnqueteur",
                table: "Affaires",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
