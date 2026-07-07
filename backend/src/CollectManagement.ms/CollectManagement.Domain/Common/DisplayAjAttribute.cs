namespace CollectManagement.Domain.Common;

public class DisplayAjAttribute  : Attribute
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ForegroundColor { get; set; }
    public string? BackgroundColor { get; set; }
}