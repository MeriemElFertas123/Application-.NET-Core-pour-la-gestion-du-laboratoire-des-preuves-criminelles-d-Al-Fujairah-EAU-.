using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class echantttttt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Echantillons_AspNetUsers_CreateurId",
                table: "Echantillons");

            migrationBuilder.DropForeignKey(
                name: "FK_RapportAnalyses_Echantillons_EchantillonId",
                table: "RapportAnalyses");

            migrationBuilder.DropForeignKey(
                name: "FK_Stockages_Echantillons_EchantillonId",
                table: "Stockages");

            migrationBuilder.DropIndex(
                name: "IX_Stockages_EchantillonId",
                table: "Stockages");

            migrationBuilder.DropIndex(
                name: "IX_RapportAnalyses_EchantillonId",
                table: "RapportAnalyses");

            migrationBuilder.DropColumn(
                name: "EchantillonId",
                table: "RapportAnalyses");

            migrationBuilder.AlterColumn<string>(
                name: "TypeMime",
                table: "Echantillons",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ResponsableCollecte",
                table: "Echantillons",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroAffaire",
                table: "Echantillons",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "NotesSupplementaires",
                table: "Echantillons",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NomFichier",
                table: "Echantillons",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LieuCollecte",
                table: "Echantillons",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Echantillons",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "CreateurId",
                table: "Echantillons",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<byte[]>(
                name: "ContenuFichier",
                table: "Echantillons",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ConditionsStockage",
                table: "Echantillons",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "CommentairesAnalyste",
                table: "Echantillons",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "CheminFichier",
                table: "Echantillons",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "QRCode",
                table: "Echantillons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StockageId",
                table: "Echantillons",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Echantillons_StockageId",
                table: "Echantillons",
                column: "StockageId",
                unique: true,
                filter: "[StockageId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Echantillons_AspNetUsers_CreateurId",
                table: "Echantillons",
                column: "CreateurId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Echantillons_Stockages_StockageId",
                table: "Echantillons",
                column: "StockageId",
                principalTable: "Stockages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Echantillons_AspNetUsers_CreateurId",
                table: "Echantillons");

            migrationBuilder.DropForeignKey(
                name: "FK_Echantillons_Stockages_StockageId",
                table: "Echantillons");

            migrationBuilder.DropIndex(
                name: "IX_Echantillons_StockageId",
                table: "Echantillons");

            migrationBuilder.DropColumn(
                name: "QRCode",
                table: "Echantillons");

            migrationBuilder.DropColumn(
                name: "StockageId",
                table: "Echantillons");

            migrationBuilder.AddColumn<int>(
                name: "EchantillonId",
                table: "RapportAnalyses",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TypeMime",
                table: "Echantillons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ResponsableCollecte",
                table: "Echantillons",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroAffaire",
                table: "Echantillons",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NotesSupplementaires",
                table: "Echantillons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NomFichier",
                table: "Echantillons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LieuCollecte",
                table: "Echantillons",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Echantillons",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreateurId",
                table: "Echantillons",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "ContenuFichier",
                table: "Echantillons",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConditionsStockage",
                table: "Echantillons",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CommentairesAnalyste",
                table: "Echantillons",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CheminFichier",
                table: "Echantillons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stockages_EchantillonId",
                table: "Stockages",
                column: "EchantillonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RapportAnalyses_EchantillonId",
                table: "RapportAnalyses",
                column: "EchantillonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Echantillons_AspNetUsers_CreateurId",
                table: "Echantillons",
                column: "CreateurId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RapportAnalyses_Echantillons_EchantillonId",
                table: "RapportAnalyses",
                column: "EchantillonId",
                principalTable: "Echantillons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Stockages_Echantillons_EchantillonId",
                table: "Stockages",
                column: "EchantillonId",
                principalTable: "Echantillons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
