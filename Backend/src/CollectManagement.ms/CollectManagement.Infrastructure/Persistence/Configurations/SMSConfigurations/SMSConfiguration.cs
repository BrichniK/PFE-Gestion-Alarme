using CollectManagement.Domain.SMS;
using CollectManagement.Domain.SMS.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectManagement.Infrastructure.Persistence.Configurations.SMSConfigurations;

public class SMSConfiguration : IEntityTypeConfiguration<SMS>
{
    public void Configure(EntityTypeBuilder<SMS> builder)
    {
        builder.HasKey(x => x.SMSId);
        
        builder.Property(p => p.SMSId)
            .HasConversion(c => c.Value.ToGuid(),
                value => new SMSId(new Ulid(value)));
        
        builder.Property(x => x.NomPrenom)
            .HasColumnType("nvarchar(200)")
            .IsRequired();
        
        builder.Property(x => x.PhoneNumber)
            .HasColumnType("nvarchar(20)")
            .IsRequired();
        
        builder.HasMany(s => s.SMSDevices)
            .WithOne(sd => sd.SMS)
            .HasForeignKey(sd => sd.SMSId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
