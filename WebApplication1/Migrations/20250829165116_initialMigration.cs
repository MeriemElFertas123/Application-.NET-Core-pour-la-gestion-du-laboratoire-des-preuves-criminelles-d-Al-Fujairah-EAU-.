using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class initialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Affaires",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOuverture = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateFermeture = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Statut = table.Column<int>(type: "int", nullable: false),
                    IdEnqueteurResponsable = table.Column<int>(type: "int", nullable: false),
                    MotifsRecurrents = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priorite = table.Column<int>(type: "int", nullable: false),
                    Lieu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumeroAffaire = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomEnqueteur = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Affaires", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmploiTemps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroSemaine = table.Column<int>(type: "int", nullable: false),
                    Annee = table.Column<int>(type: "int", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Commentaires = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateApprobation = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreePar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApprouvePar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiePar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploiTemps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    permission = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CapaciteMaximum = table.Column<int>(type: "int", nullable: false),
                    CodeCouleur = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Actif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TypesAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NiveauAcces = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypesAnalyses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TypesEchantillons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Stockage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Temperature = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypesEchantillons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZoneStockages",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Temperature = table.Column<int>(type: "int", nullable: false),
                    Capacite = table.Column<int>(type: "int", nullable: false),
                    Occupe = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZoneStockages", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "EnvoiComplets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AffaireId = table.Column<int>(type: "int", nullable: false),
                    TypeAnalyseDemandee = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateEnvoiPrevue = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEnvoiEffective = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatutEnvoi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ObservationsEnvoi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EchantillonId = table.Column<int>(type: "int", nullable: false),
                    CodeQR = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Poids = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConditionsStockage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Couleur = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Emballage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Emplacement = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ObservationsEchantillon = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    StatutEchantillon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DatePreparation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateReception = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateVerification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiePar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NotesVerification = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnvoiComplets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnvoiComplets_Affaires_AffaireId",
                        column: x => x.AffaireId,
                        principalTable: "Affaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Envois",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AffaireId = table.Column<int>(type: "int", nullable: false),
                    TypeAnalyseDemandee = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateEnvoiPrevue = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEnvoiEffective = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateReception = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatutEnvoi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Envois", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Envois_Affaires_AffaireId",
                        column: x => x.AffaireId,
                        principalTable: "Affaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistoriqueAffaires",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AffaireId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MotifCloture = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Commentaire = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DateAction = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    NomUtilisateur = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TypeAction = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DetailsTechniques = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoriqueAffaires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoriqueAffaires_Affaires_AffaireId",
                        column: x => x.AffaireId,
                        principalTable: "Affaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RapportCloture",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AffaireId = table.Column<int>(type: "int", nullable: false),
                    DateCloture = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResumeActions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResultatsObtenus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Recommandations = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NotesComplementaires = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuteurId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RapportCloture", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RapportCloture_Affaires_AffaireId",
                        column: x => x.AffaireId,
                        principalTable: "Affaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nom = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    prenom = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Analystes",
                columns: table => new
                {
                    idAnalyste = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    prenom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    specialite = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    laboratoire = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChargeActuelle = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analystes", x => x.idAnalyste);
                    table.ForeignKey(
                        name: "FK_Analystes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enqueteurs",
                columns: table => new
                {
                    idEnqueteur = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    prenom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    grade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    service = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enqueteurs", x => x.idEnqueteur);
                    table.ForeignKey(
                        name: "FK_Enqueteurs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Techniciens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MotDePasse = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Techniciens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Techniciens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DroitId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPermissions",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissions", x => new { x.UserId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_UserPermissions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Personnels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TypePersonnel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telephone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Actif = table.Column<bool>(type: "bit", nullable: false),
                    DateEmbauche = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personnels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Personnels_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RapportsNonConformite",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnvoiEchantillonId = table.Column<int>(type: "int", nullable: false),
                    NumeroRapport = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateRapport = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RedacteurRapport = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DescriptionNonConformite = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CausesProbables = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ActionRecommandee = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NotesDetaillees = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    StatutRapport = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EnvoiCompletId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RapportsNonConformite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RapportsNonConformite_EnvoiComplets_EnvoiCompletId",
                        column: x => x.EnvoiCompletId,
                        principalTable: "EnvoiComplets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Echantillons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroEchantillon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumeroAffaire = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AffaireId = table.Column<int>(type: "int", nullable: true),
                    CreateurId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AnalysteId = table.Column<int>(type: "int", nullable: true),
                    DejaEnvoye = table.Column<bool>(type: "bit", nullable: false),
                    EnvoiCompletId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Statut = table.Column<int>(type: "int", nullable: false),
                    Priorite = table.Column<int>(type: "int", nullable: false),
                    DateReception = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateDebutAnalyse = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateFinAnalyse = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateLimite = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateArchivage = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ConditionsStockage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LieuCollecte = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ResponsableCollecte = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NomFichier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CheminFichier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContenuFichier = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    TypeMime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TailleFichier = table.Column<long>(type: "bigint", nullable: false),
                    EnqueteuridEnqueteur = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Echantillons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Echantillons_Affaires_AffaireId",
                        column: x => x.AffaireId,
                        principalTable: "Affaires",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Echantillons_Analystes_AnalysteId",
                        column: x => x.AnalysteId,
                        principalTable: "Analystes",
                        principalColumn: "idAnalyste");
                    table.ForeignKey(
                        name: "FK_Echantillons_AspNetUsers_CreateurId",
                        column: x => x.CreateurId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Echantillons_Enqueteurs_EnqueteuridEnqueteur",
                        column: x => x.EnqueteuridEnqueteur,
                        principalTable: "Enqueteurs",
                        principalColumn: "idEnqueteur");
                    table.ForeignKey(
                        name: "FK_Echantillons_EnvoiComplets_EnvoiCompletId",
                        column: x => x.EnvoiCompletId,
                        principalTable: "EnvoiComplets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AbsencesConges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonnelId = table.Column<int>(type: "int", nullable: false),
                    EmploiTempsId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateDebut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Motif = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RemplacantId = table.Column<int>(type: "int", nullable: true),
                    Statut = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DateDemande = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbsencesConges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbsencesConges_EmploiTemps_EmploiTempsId",
                        column: x => x.EmploiTempsId,
                        principalTable: "EmploiTemps",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AbsencesConges_Personnels_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AbsencesConges_Personnels_RemplacantId",
                        column: x => x.RemplacantId,
                        principalTable: "Personnels",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AffectationsPersonnel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmploiTempsId = table.Column<int>(type: "int", nullable: false),
                    PersonnelId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    Jour = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Equipe = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HeureDebut = table.Column<TimeSpan>(type: "time", nullable: false),
                    HeureFin = table.Column<TimeSpan>(type: "time", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AffectationsPersonnel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AffectationsPersonnel_EmploiTemps_EmploiTempsId",
                        column: x => x.EmploiTempsId,
                        principalTable: "EmploiTemps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AffectationsPersonnel_Personnels_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AffectationsPersonnel_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Analyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EstValide = table.Column<bool>(type: "bit", nullable: false),
                    EchantillonId = table.Column<int>(type: "int", nullable: false),
                    AnalysteId = table.Column<int>(type: "int", nullable: false),
                    DateAnalyse = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TypeAnalyse = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Methode = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Conclusion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Resultats = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    NomFichier = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FichierContenu = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    FichierContentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Statut = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Analyses_Echantillons_EchantillonId",
                        column: x => x.EchantillonId,
                        principalTable: "Echantillons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Archives",
                columns: table => new
                {
                    idArchive = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dateArchivage = table.Column<DateTime>(type: "datetime2", nullable: false),
                    emplacementPhysique = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    codeQR = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    idPreuve = table.Column<int>(type: "int", nullable: false),
                    preuveId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Archives", x => x.idArchive);
                    table.ForeignKey(
                        name: "FK_Archives_Echantillons_preuveId",
                        column: x => x.preuveId,
                        principalTable: "Echantillons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttributionAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EchantillonId = table.Column<int>(type: "int", nullable: false),
                    AnalysteId = table.Column<int>(type: "int", nullable: false),
                    TypeAnalyseRequis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrioriteAnalyse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InstructionsSpeciales = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateAttribution = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AttributePar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NotificationEnvoyee = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributionAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttributionAnalyses_Analystes_AnalysteId",
                        column: x => x.AnalysteId,
                        principalTable: "Analystes",
                        principalColumn: "idAnalyste",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttributionAnalyses_Echantillons_EchantillonId",
                        column: x => x.EchantillonId,
                        principalTable: "Echantillons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Emplacements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Zone = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EstOccupe = table.Column<bool>(type: "bit", nullable: false),
                    EchantillonId = table.Column<int>(type: "int", nullable: true),
                    DateOccupation = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ZoneStockageCode = table.Column<string>(type: "nvarchar(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emplacements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Emplacements_Echantillons_EchantillonId",
                        column: x => x.EchantillonId,
                        principalTable: "Echantillons",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Emplacements_ZoneStockages_ZoneStockageCode",
                        column: x => x.ZoneStockageCode,
                        principalTable: "ZoneStockages",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnvoiEchantillons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnvoiId = table.Column<int>(type: "int", nullable: false),
                    EchantillonId = table.Column<int>(type: "int", nullable: false),
                    CodeQR = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Poids = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConditionsStockage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Couleur = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Emballage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Emplacement = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    StatutEchantillon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DatePreparation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateVerification = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiePar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NotesVerification = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DateReception = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnvoiEchantillons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnvoiEchantillons_Echantillons_EchantillonId",
                        column: x => x.EchantillonId,
                        principalTable: "Echantillons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnvoiEchantillons_Envois_EnvoiId",
                        column: x => x.EnvoiId,
                        principalTable: "Envois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Stockages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EchantillonId = table.Column<int>(type: "int", nullable: false),
                    Zone = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Emplacement = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DateStockage = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TechnicienId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Statut = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stockages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stockages_Echantillons_EchantillonId",
                        column: x => x.EchantillonId,
                        principalTable: "Echantillons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RapportAnalyses",
                columns: table => new
                {
                    idRapport = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    conclusion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dateRapport = table.Column<DateTime>(type: "datetime2", nullable: false),
                    idPreuve = table.Column<int>(type: "int", nullable: false),
                    idAnalyste = table.Column<int>(type: "int", nullable: false),
                    AnalyseId = table.Column<int>(type: "int", nullable: true),
                    EchantillonId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RapportAnalyses", x => x.idRapport);
                    table.ForeignKey(
                        name: "FK_RapportAnalyses_Analyses_AnalyseId",
                        column: x => x.AnalyseId,
                        principalTable: "Analyses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RapportAnalyses_Analystes_idAnalyste",
                        column: x => x.idAnalyste,
                        principalTable: "Analystes",
                        principalColumn: "idAnalyste",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RapportAnalyses_Echantillons_EchantillonId",
                        column: x => x.EchantillonId,
                        principalTable: "Echantillons",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RapportAnalyses_Echantillons_idPreuve",
                        column: x => x.idPreuve,
                        principalTable: "Echantillons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VerificationEchantillons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnvoiEchantillonId = table.Column<int>(type: "int", nullable: false),
                    Critere = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ValeurAttendue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ValeurConstatee = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EstConforme = table.Column<bool>(type: "bit", nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DateVerification = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VerifiePar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationEchantillons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerificationEchantillons_EnvoiEchantillons_EnvoiEchantillonId",
                        column: x => x.EnvoiEchantillonId,
                        principalTable: "EnvoiEchantillons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbsencesConges_EmploiTempsId",
                table: "AbsencesConges",
                column: "EmploiTempsId");

            migrationBuilder.CreateIndex(
                name: "IX_AbsencesConges_PersonnelId",
                table: "AbsencesConges",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_AbsencesConges_RemplacantId",
                table: "AbsencesConges",
                column: "RemplacantId");

            migrationBuilder.CreateIndex(
                name: "IX_Admins_UserId",
                table: "Admins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AffectationsPersonnel_EmploiTempsId",
                table: "AffectationsPersonnel",
                column: "EmploiTempsId");

            migrationBuilder.CreateIndex(
                name: "IX_AffectationsPersonnel_PersonnelId",
                table: "AffectationsPersonnel",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_AffectationsPersonnel_ServiceId",
                table: "AffectationsPersonnel",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Analyses_EchantillonId",
                table: "Analyses",
                column: "EchantillonId");

            migrationBuilder.CreateIndex(
                name: "IX_Analystes_UserId",
                table: "Analystes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Archives_preuveId",
                table: "Archives",
                column: "preuveId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AttributionAnalyses_AnalysteId",
                table: "AttributionAnalyses",
                column: "AnalysteId");

            migrationBuilder.CreateIndex(
                name: "IX_AttributionAnalyses_EchantillonId",
                table: "AttributionAnalyses",
                column: "EchantillonId");

            migrationBuilder.CreateIndex(
                name: "IX_Echantillon_NumeroEchantillon",
                table: "Echantillons",
                column: "NumeroEchantillon",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Echantillons_AffaireId",
                table: "Echantillons",
                column: "AffaireId");

            migrationBuilder.CreateIndex(
                name: "IX_Echantillons_AnalysteId",
                table: "Echantillons",
                column: "AnalysteId");

            migrationBuilder.CreateIndex(
                name: "IX_Echantillons_CreateurId",
                table: "Echantillons",
                column: "CreateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Echantillons_EnqueteuridEnqueteur",
                table: "Echantillons",
                column: "EnqueteuridEnqueteur");

            migrationBuilder.CreateIndex(
                name: "IX_Echantillons_EnvoiCompletId",
                table: "Echantillons",
                column: "EnvoiCompletId");

            migrationBuilder.CreateIndex(
                name: "IX_Emplacements_EchantillonId",
                table: "Emplacements",
                column: "EchantillonId");

            migrationBuilder.CreateIndex(
                name: "IX_Emplacements_ZoneStockageCode",
                table: "Emplacements",
                column: "ZoneStockageCode");

            migrationBuilder.CreateIndex(
                name: "IX_Enqueteurs_UserId",
                table: "Enqueteurs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EnvoiComplets_AffaireId",
                table: "EnvoiComplets",
                column: "AffaireId");

            migrationBuilder.CreateIndex(
                name: "IX_EnvoiEchantillons_EchantillonId",
                table: "EnvoiEchantillons",
                column: "EchantillonId");

            migrationBuilder.CreateIndex(
                name: "IX_EnvoiEchantillons_EnvoiId",
                table: "EnvoiEchantillons",
                column: "EnvoiId");

            migrationBuilder.CreateIndex(
                name: "IX_Envois_AffaireId",
                table: "Envois",
                column: "AffaireId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoriqueAffaires_AffaireId",
                table: "HistoriqueAffaires",
                column: "AffaireId");

            migrationBuilder.CreateIndex(
                name: "IX_Personnels_ServiceId",
                table: "Personnels",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_RapportAnalyses_AnalyseId",
                table: "RapportAnalyses",
                column: "AnalyseId");

            migrationBuilder.CreateIndex(
                name: "IX_RapportAnalyses_EchantillonId",
                table: "RapportAnalyses",
                column: "EchantillonId");

            migrationBuilder.CreateIndex(
                name: "IX_RapportAnalyses_idAnalyste",
                table: "RapportAnalyses",
                column: "idAnalyste");

            migrationBuilder.CreateIndex(
                name: "IX_RapportAnalyses_idPreuve",
                table: "RapportAnalyses",
                column: "idPreuve");

            migrationBuilder.CreateIndex(
                name: "IX_RapportCloture_AffaireId",
                table: "RapportCloture",
                column: "AffaireId");

            migrationBuilder.CreateIndex(
                name: "IX_RapportsNonConformite_EnvoiCompletId",
                table: "RapportsNonConformite",
                column: "EnvoiCompletId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Stockages_EchantillonId",
                table: "Stockages",
                column: "EchantillonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Techniciens_UserId",
                table: "Techniciens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_PermissionId",
                table: "UserPermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationEchantillons_EnvoiEchantillonId",
                table: "VerificationEchantillons",
                column: "EnvoiEchantillonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbsencesConges");

            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "AffectationsPersonnel");

            migrationBuilder.DropTable(
                name: "Archives");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AttributionAnalyses");

            migrationBuilder.DropTable(
                name: "Emplacements");

            migrationBuilder.DropTable(
                name: "HistoriqueAffaires");

            migrationBuilder.DropTable(
                name: "RapportAnalyses");

            migrationBuilder.DropTable(
                name: "RapportCloture");

            migrationBuilder.DropTable(
                name: "RapportsNonConformite");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "Stockages");

            migrationBuilder.DropTable(
                name: "Techniciens");

            migrationBuilder.DropTable(
                name: "TypesAnalyses");

            migrationBuilder.DropTable(
                name: "TypesEchantillons");

            migrationBuilder.DropTable(
                name: "UserPermissions");

            migrationBuilder.DropTable(
                name: "VerificationEchantillons");

            migrationBuilder.DropTable(
                name: "EmploiTemps");

            migrationBuilder.DropTable(
                name: "Personnels");

            migrationBuilder.DropTable(
                name: "ZoneStockages");

            migrationBuilder.DropTable(
                name: "Analyses");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "EnvoiEchantillons");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Echantillons");

            migrationBuilder.DropTable(
                name: "Envois");

            migrationBuilder.DropTable(
                name: "Analystes");

            migrationBuilder.DropTable(
                name: "Enqueteurs");

            migrationBuilder.DropTable(
                name: "EnvoiComplets");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Affaires");
        }
    }
}
