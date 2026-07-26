using CollectManagementDomain.Societes;
using CollectManagementDomain.Societes.ValueObjects;

namespace CollectManagement.Infrastructure.Persistence.Configurations.SocieteConfigurations;

public class SocieteConfiguration :IEntityTypeConfiguration<Societe>
{
    public void Configure(EntityTypeBuilder<Societe> builder)
    {
        builder.HasKey(c => c.SocieteId);

        builder.Property(c => c.SocieteId)
            .HasConversion(c => c.Value.ToGuid(),
                value => new SocieteId(new Ulid(value)));
        
        builder.Property(p => p.LogoPath)
            .HasMaxLength(255);

        builder.Property(p => p.Nom)
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(p=>p.MatriculeFiscal)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Rne)
            .HasMaxLength(50);
        
        builder.Property(e=>e.DateOverture)
            .HasColumnType("date")
            .IsRequired();
        
        builder.Property(v=>v.Capital)
            .HasColumnType("decimal(18,3)")
            .HasDefaultValue(0m)
            .IsRequired();

        builder.Property(p => p.Telephone1)
            .HasMaxLength(13);
        
        builder.Property(p => p.Telephone2)
            .HasMaxLength(13);
        
        builder.Property(p => p.Fax1)
            .HasMaxLength(13);
        
        builder.Property(p => p.Fax2)
            .HasMaxLength(13);
        
        builder.Property(p => p.Email)
            .HasMaxLength(30);
        
        builder.Property(p => p.Adresse)
            .HasMaxLength(50);
        
        builder.Property(p => p.CodeSociete)
            .HasMaxLength(50);
    }
}