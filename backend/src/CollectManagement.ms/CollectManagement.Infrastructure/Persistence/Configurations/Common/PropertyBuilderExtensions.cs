using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CollectManagement.Infrastructure.Persistence.Configurations.Common;

internal static class PropertyBuilderExtensions
{
    public static PropertyBuilder<IEnumerable<TEnum>> HasEnumCollectionConversion<TEnum>(
        this PropertyBuilder<IEnumerable<TEnum>> propertyBuilder)
        where TEnum : struct, Enum
    {
        var converter = new ValueConverter<IEnumerable<TEnum>, string>(
            v => string.Join(",", v.Select(e => e.ToString())),
            v => string.IsNullOrEmpty(v)
                ? new List<TEnum>()
                : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(Enum.Parse<TEnum>)
                    .ToList());

        var comparer = new ValueComparer<IEnumerable<TEnum>>(
            (l, r) =>
                ReferenceEquals(l, r) ||
                (l != null && r != null && l.SequenceEqual(r)),
            v => v.Aggregate(0, (a, e) => HashCode.Combine(a, e.GetHashCode())),
            v => v.ToList()
        );

        propertyBuilder.HasConversion(converter);
        propertyBuilder.Metadata.SetValueComparer(comparer);

        return propertyBuilder;
    }
}