using CollectManagement.Domain.SMSConfigurations;
using CollectManagement.Domain.SMSConfigurations.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectManagement.Infrastructure.Persistence.Configurations.SMSConfigurationConfigurations;

public class SMSConfigurationEntityConfiguration : IEntityTypeConfiguration<SMSConfiguration>
{
    public void Configure(EntityTypeBuilder<SMSConfiguration> builder)
    {
        builder.HasKey(x => x.SMSConfigurationId);

        builder.Property(p => p.SMSConfigurationId)
            .HasConversion(c => c.Value.ToGuid(),
                value => new SMSConfigurationId(new Ulid(value)));

        builder.Property(x => x.ApiUrl)
            .HasColumnType("nvarchar(500)")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.NombreAlerte)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(x => x.Delai)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.SmsOnAlerte)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.SmsOnBadgeT3)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.SmsOnBadgeT4)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.SmsOnBadgeT5)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.SmsOnTraitement)
            .IsRequired()
            .HasDefaultValue(true);
    }
}
