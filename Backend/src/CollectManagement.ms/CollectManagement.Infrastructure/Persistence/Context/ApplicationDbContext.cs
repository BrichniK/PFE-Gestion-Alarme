using CollectManagement.Domain.Alertes;
using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.Entities;
using CollectManagement.Infrastructure.Persistence.Configurations.AlerteConfigurations;
using CollectManagementDomain.Societes;
using Microsoft.EntityFrameworkCore;

namespace CollectManagement.Infrastructure.Persistence.Context;

public class ApplicationDbContext : DbContext
{ public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Utilisateur> Utilisateurs { get; set; }
    
    public DbSet<RoleUtilisateur> RoleUtilisateurs { get; set; }

    public DbSet<Societe> Societes { get; set; }
    
    public DbSet<Alerte> Alertes { get; set; }

    public DbSet<Device> Devices { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly
        );

        base.OnModelCreating(modelBuilder);
    }
}