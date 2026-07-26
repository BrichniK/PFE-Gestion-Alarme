using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using CollectManagement.Domain.Common;

namespace CollectManagement.Application.Common;

public static class EnumHelper
{
    public static List<EnumInfo> SmartEnumToList<TEnum>() where TEnum : Enum
    {
        return Enum.GetValues(typeof(TEnum))
            .Cast<TEnum>()
            .Select(GetEnumInfo)
            .ToList();
    }
    
    private static string GetEnumDescription<TEnum>(TEnum value) where TEnum : Enum
    {
        var field = value.GetType().GetField(value.ToString()) ?? null;
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute == null ? value.ToString() : attribute.Description;
    }

    private static EnumInfo GetEnumInfo<TEnum>(TEnum value) where TEnum : Enum
    {
        var type = typeof(TEnum); 
        var memberInfo = type.GetMember(value.ToString()).FirstOrDefault(); 
        var displayAttribute = memberInfo?.GetCustomAttribute<DisplayAttribute>();
        var displayAjAttribute = memberInfo?.GetCustomAttribute<DisplayAjAttribute>();
        
        return new EnumInfo
        {
            Id = Convert.ToInt32(value, CultureInfo.InvariantCulture),
            Value = displayAttribute?.Name ?? displayAjAttribute?.Name ?? value.ToString(),
            TextColor = displayAttribute?.ShortName ?? displayAjAttribute?.ForegroundColor ?? "",
            Color = displayAttribute?.GroupName ?? displayAjAttribute?.BackgroundColor ?? ""
        };
    }
}


public class EnumInfo
{
    public int Id { get; set; }
    public string? Value { get; set; }
    public string? TextColor { get; set; }
    public string? Color { get; set; }
}
