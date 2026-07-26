using CollectManagement.Domain.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.Entities;
using CollectManagementDomain.Societes;
using Microsoft.EntityFrameworkCore;

namespace CollectManagement.Infrastructure.Persistence.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Utilisateur> Utilisateurs { get; set; }
    
    public DbSet<RoleUtilisateur> RoleUtilisateurs { get; set; }

    public DbSet<Societe> Societes { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Charge toutes les configurations EF Core
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}