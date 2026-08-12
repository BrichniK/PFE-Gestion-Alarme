using CollectManagement.Domain.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.ValueObjects;
using CollectManagementDomain.Societes.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectManagement.Infrastructure.Persistence.Configurations.UtilisateurConfigurations;

public class UtilisateurConfiguration
    : IEntityTypeConfiguration<Utilisateur>
{
    public void Configure(EntityTypeBuilder<Utilisateur> builder)
    {
        builder.HasKey(x => x.UtilisateurId);
        
        builder.Property(p => p.UtilisateurId)
            .HasConversion(c => c.Value.ToGuid(),
                value => new UtilisateurId(new Ulid(value)));
        
        builder.Property(x => x.NomUtilisateur)
            .HasColumnType("nvarchar(20)")
            .IsRequired();
        
        builder.Property(x => x.Nom)
            .HasColumnType("nvarchar(50)")
            .IsRequired();
        
        builder.Property(x => x.Prenom)
            .HasColumnType("nvarchar(50)")
            .IsRequired();
        
        builder.Property(x => x.Email)
            .HasColumnType("nvarchar(100)")
            .IsRequired();
        
        builder.Property(x => x.Password)
            .IsRequired();
        
        builder.Property(p => p.RoleUtilisateurId)
            .HasConversion(c => c == null ? null : (Guid?)c.Value.ToGuid(),
                value => value.HasValue ? new RoleUtilisateurId(new Ulid(value.Value)) : null);
        
        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .IsRequired()
            .ValueGeneratedNever();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.HasIndex(x => x.NomUtilisateur)
            .IsUnique();
        
        builder.Property(s=>s.SocieteId)
            .HasConversion(s=>s.Value.ToGuid(),
                value => new SocieteId(new Ulid(value)))
            .IsRequired(false);
        
        builder.HasOne(c => c.Societe)
            .WithMany()
            .HasForeignKey(k => k.SocieteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.RoleUtilisateur)
            .WithMany()
            .HasForeignKey(k => k.RoleUtilisateurId)
            .OnDelete(DeleteBehavior.Restrict);
        
      
    }
}