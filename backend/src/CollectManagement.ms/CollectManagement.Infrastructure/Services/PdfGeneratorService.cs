using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Infrastructure.PuppeteerConfig;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace CollectManagement.Infrastructure.Services;

public class PdfGeneratorService : IPdfGeneratorService
{
    private readonly PuppeteerOptions _puppeteerOptions;

    public PdfGeneratorService(
        IOptions<PuppeteerOptions> puppeteerOptions)
    {
        _puppeteerOptions = puppeteerOptions.Value;
    }

    public async Task<byte[]> GeneratePdfFromHtml(string html)
    {
        // Set up Puppeteer
        var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            //this is he default chrome.exe path
            ExecutablePath = string.IsNullOrEmpty(_puppeteerOptions.ChromePath) 
                ? string.Empty 
                : _puppeteerOptions.ChromePath // @"C:\Program Files\Google\Chrome\Application\chrome.exe"
        }).ConfigureAwait(false);

        var page = await browser.NewPageAsync().ConfigureAwait(false);
        
        // Set the HTML content to the page
        await page.SetContentAsync(html).ConfigureAwait(false);

        // Generate PDF
        var pdfStream = await page.PdfDataAsync(new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            DisplayHeaderFooter = true,
            FooterTemplate = """
                             <div style='width: 100%; text-align: center; font-size: 12px; color: #555;'> 
                                <span class='pageNumber'></span> / <span class='totalPages'></span>
                             </div>
                             """,
            MarginOptions = new MarginOptions
            {
                Bottom = "40px",
            }
        }).ConfigureAwait(false);

        // Close the browser
        await browser.CloseAsync().ConfigureAwait(false);
        
        return pdfStream;
    }
}