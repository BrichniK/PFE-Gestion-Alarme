
using CollectManagement.Domain.Alertes;
using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Types;
using CollectManagement.Domain.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.Entities;
using CollectManagementDomain.Societes;
using Microsoft.EntityFrameworkCore;
using Type = CollectManagement.Domain.Types.Type;

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

    public DbSet<Alerte> Alertes { get; set; }

    public DbSet<Device> Devices { get; set; }

    public DbSet<Type> Types { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Charge automatiquement toutes les configurations
        // IEntityTypeConfiguration<T> présentes dans l'assembly Infrastructure.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }
}
