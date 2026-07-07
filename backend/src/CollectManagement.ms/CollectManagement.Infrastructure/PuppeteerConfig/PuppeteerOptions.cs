namespace CollectManagement.Infrastructure.PuppeteerConfig;

public class PuppeteerOptions
{
    public static string SectionName { get; } = "Puppeteer";
    public string ChromePath { get; set; }
}