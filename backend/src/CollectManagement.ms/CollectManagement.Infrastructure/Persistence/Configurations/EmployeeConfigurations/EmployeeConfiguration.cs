using CollectManagement.Domain.Employess;
using CollectManagement.Domain.Employess.ObjectValues;

namespace CollectManagement.Infrastructure.Persistence.Configurations.EmployeeConfigurations;

public class EmployeeConfiguration :IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(y => y.EmployeeId);

        builder.Property(y => y.EmployeeId)
            .HasConversion(v => v.Value.ToGuid(),
                value => new EmployeeId(new Ulid(value)));
        
        builder.Property(p=>p.Nom)
            .HasColumnType("varchar(100)")
            .IsRequired();
        
        builder.Property(p=>p.Prenom)
            .HasColumnType("varchar(100)")
            .IsRequired();
        
        builder.Property(p=>p.Phone)
            .IsRequired();
        
        builder.Property(p=>p.Rfid)
            .HasColumnType("varchar(100)")
            .IsRequired();
        
        builder.Property(p => p.Email)
            .HasColumnType("varchar(255)");
        
        builder.Property(p => p.LogoPath)
            .HasMaxLength(255);
    }
}