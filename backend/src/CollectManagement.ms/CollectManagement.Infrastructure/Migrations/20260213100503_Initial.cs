using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollectManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoleUtilisateur",
                columns: table => new
                {
                    RoleUtilisateurId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LibelleRoleUtilisateur = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleUtilisateur", x => x.RoleUtilisateurId);
                });

            migrationBuilder.CreateTable(
                name: "Societe",
                columns: table => new
                {
                    SocieteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LogoPath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Nom = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatriculeFiscal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Rne = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Capital = table.Column<decimal>(type: "decimal(18,3)", nullable: false, defaultValue: 0m),
                    DateOverture = table.Column<DateTime>(type: "date", nullable: false),
                    Telephone1 = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    Telephone2 = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    Fax1 = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    Fax2 = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Adresse = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CodeSociete = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Societe", x => x.SocieteId);
                });

            migrationBuilder.CreateTable(
                name: "Navigation",
                columns: table => new
                {
                    NavigationId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    RoleUtilisateurId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Actions = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Navigation", x => new { x.NavigationId, x.RoleUtilisateurId });
                    table.ForeignKey(
                        name: "FK_Navigation_RoleUtilisateur_RoleUtilisateurId",
                        column: x => x.RoleUtilisateurId,
                        principalTable: "RoleUtilisateur",
                        principalColumn: "RoleUtilisateurId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateur",
                columns: table => new
                {
                    UtilisateurId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomUtilisateur = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoleUtilisateurId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SocieteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InsererPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsertion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierPar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModification = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateur", x => x.UtilisateurId);
                    table.ForeignKey(
                        name: "FK_Utilisateur_RoleUtilisateur_RoleUtilisateurId",
                        column: x => x.RoleUtilisateurId,
                        principalTable: "RoleUtilisateur",
                        principalColumn: "RoleUtilisateurId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Utilisateur_Societe_SocieteId",
                        column: x => x.SocieteId,
                        principalTable: "Societe",
                        principalColumn: "SocieteId");
                });

            migrationBuilder.CreateTable(
                name: "NavigationSection",
                columns: table => new
                {
                    SectionId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    Actions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NavigationId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    NavigationRoleUtilisateurId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavigationSection", x => x.SectionId);
                    table.ForeignKey(
                        name: "FK_NavigationSection_Navigation_NavigationId_NavigationRoleUtilisateurId",
                        columns: x => new { x.NavigationId, x.NavigationRoleUtilisateurId },
                        principalTable: "Navigation",
                        principalColumns: new[] { "NavigationId", "RoleUtilisateurId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Navigation_RoleUtilisateurId",
                table: "Navigation",
                column: "RoleUtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_NavigationSection_NavigationId_NavigationRoleUtilisateurId",
                table: "NavigationSection",
                columns: new[] { "NavigationId", "NavigationRoleUtilisateurId" });

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateur_Email",
                table: "Utilisateur",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateur_NomUtilisateur",
                table: "Utilisateur",
                column: "NomUtilisateur",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateur_RoleUtilisateurId",
                table: "Utilisateur",
                column: "RoleUtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateur_SocieteId",
                table: "Utilisateur",
                column: "SocieteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NavigationSection");

            migrationBuilder.DropTable(
                name: "Utilisateur");

            migrationBuilder.DropTable(
                name: "Navigation");

            migrationBuilder.DropTable(
                name: "Societe");

            migrationBuilder.DropTable(
                name: "RoleUtilisateur");
        }
    }
}
