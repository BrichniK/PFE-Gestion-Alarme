using CollectManagement.Domain.Utilisateurs.Entities;
using CollectManagement.Domain.Utilisateurs.ValueObjects;
using CollectManagement.Infrastructure.Persistence.Configurations.Common;

namespace CollectManagement.Infrastructure.Persistence.Configurations.UtilisateurConfigurations;

public class RoleUtilisateurConfiguration : IEntityTypeConfiguration<RoleUtilisateur>
{
    public void Configure(EntityTypeBuilder<RoleUtilisateur> builder)
    {
        builder.HasKey(x => x.RoleUtilisateurId);
        
        builder.Property(p => p.RoleUtilisateurId)
            .HasConversion(c => c.Value.ToGuid(),
                value => new RoleUtilisateurId(new Ulid(value)));
        
        builder.Property(x => x.LibelleRoleUtilisateur)
            .HasColumnType("nvarchar(20)")
            .IsRequired();

        builder.OwnsMany(m => m.Navigations, navBuilder =>
        {
            navBuilder.WithOwner();

            navBuilder.HasKey(k => new {k.NavigationId, k.RoleUtilisateurId});
            
            navBuilder.Property(p => p.RoleUtilisateurId)
                .HasConversion(c => c.Value.ToGuid(),
                    value => new RoleUtilisateurId(new Ulid(value)));
            
            navBuilder.Property(n => n.NavigationId)
                .HasColumnType("nvarchar(50)")
                .IsRequired();

            navBuilder.Property(n => n.Actions)
                .HasEnumCollectionConversion();
            
            navBuilder.OwnsMany(m => m.Sections, sectionBuilder =>
            {
                sectionBuilder.WithOwner();

                sectionBuilder.HasKey(k => k.SectionId);
                
                sectionBuilder.Property(n => n.SectionId)
                    .HasColumnType("nvarchar(50)")
                    .ValueGeneratedNever()
                    .IsRequired();

                sectionBuilder.Property(n => n.Actions)
                    .HasEnumCollectionConversion();
            });
        });
    }
}