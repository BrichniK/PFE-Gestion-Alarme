using CollectManagement.Domain.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.Entities;
using CollectManagement.Domain.Utilisateurs.ValueObjects;
using CollectManagementDomain.Societes;
using CollectManagementDomain.Societes.ValueObjects;
using CollectManagement.Infrastructure.Persistence.Context;
using CollectManagement.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CollectManagement.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var passwordService = scope.ServiceProvider
            .GetRequiredService<IPasswordService>();

        await context.Database.MigrateAsync();

        if (await context.Utilisateurs.AnyAsync())
        {
            return;
        }

        var societeId = new SocieteId(
            Ulid.Parse("01HC85BM5QVRW7ABRV33TR1GQ0"));

        var societe = Societe.Create(
            societeId,
            logoPath: null,
            nom: "Canadian System Technology",
            matriculeFiscal: "0000000A",
            rne: "CST001",
            capital: 0,
            dateOverture: DateTime.Now,
            telephone1: "00000000",
            telephone2: null,
            fax1: null,
            fax2: null,
            email: "contact@cst.tn",
            adresse: "Tunisie",
            codeSociete: "CST"
        );

        context.Societes.Add(societe);

        var roleId = new RoleUtilisateurId(
            Ulid.Parse("01HC85BM5QVRW7ABRV33TR1GQ1"));

        var role = RoleUtilisateur.Create(
            roleId,
            "SuperAdmin",
            new List<Navigation>()
        );

        context.RoleUtilisateurs.Add(role);

        var utilisateurId = new UtilisateurId(
            Ulid.Parse("01HC85BM5QVRW7ABRV33TR1GQ2"));

        const string password = "root";

        var passwordHash = passwordService.HashPassword(
            utilisateurId,
            password
        );

        var utilisateur = Utilisateur.Create(
            utilisateurId,
            "root",
            "CST",
            "Admin",
            "root@cst.tn",
            passwordHash,
            roleId,
            true,
            societeId
        );

        context.Utilisateurs.Add(utilisateur);

        await context.SaveChangesAsync();
    }
}