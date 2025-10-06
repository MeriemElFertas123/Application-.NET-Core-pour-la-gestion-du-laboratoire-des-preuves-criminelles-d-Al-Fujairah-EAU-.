using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class Finaaal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AbsencesConges_EmploiTemps_EmploiTempsId",
                table: "AbsencesConges");

            migrationBuilder.DropForeignKey(
                name: "FK_AffectationsPersonnel_EmploiTemps_EmploiTempsId",
                table: "AffectationsPersonnel");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmploiTemps",
                table: "EmploiTemps");

            migrationBuilder.RenameTable(
                name: "EmploiTemps",
                newName: "EmploisTemps");

            migrationBuilder.AlterColumn<string>(
                name: "PermissionId",
                table: "RolePermissions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "PermissionId1",
                table: "RolePermissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NotesSupplementaires",
                table: "Echantillons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmploisTemps",
                table: "EmploisTemps",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId1",
                table: "RolePermissions",
                column: "PermissionId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AbsencesConges_EmploisTemps_EmploiTempsId",
                table: "AbsencesConges",
                column: "EmploiTempsId",
                principalTable: "EmploisTemps",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AffectationsPersonnel_EmploisTemps_EmploiTempsId",
                table: "AffectationsPersonnel",
                column: "EmploiTempsId",
                principalTable: "EmploisTemps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId1",
                table: "RolePermissions",
                column: "PermissionId1",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AbsencesConges_EmploisTemps_EmploiTempsId",
                table: "AbsencesConges");

            migrationBuilder.DropForeignKey(
                name: "FK_AffectationsPersonnel_EmploisTemps_EmploiTempsId",
                table: "AffectationsPersonnel");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId1",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_PermissionId1",
                table: "RolePermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmploisTemps",
                table: "EmploisTemps");

            migrationBuilder.DropColumn(
                name: "PermissionId1",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "NotesSupplementaires",
                table: "Echantillons");

            migrationBuilder.RenameTable(
                name: "EmploisTemps",
                newName: "EmploiTemps");

            migrationBuilder.AlterColumn<int>(
                name: "PermissionId",
                table: "RolePermissions",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmploiTemps",
                table: "EmploiTemps",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AbsencesConges_EmploiTemps_EmploiTempsId",
                table: "AbsencesConges",
                column: "EmploiTempsId",
                principalTable: "EmploiTemps",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AffectationsPersonnel_EmploiTemps_EmploiTempsId",
                table: "AffectationsPersonnel",
                column: "EmploiTempsId",
                principalTable: "EmploiTemps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
