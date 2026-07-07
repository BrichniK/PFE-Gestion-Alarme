using PuppeteerSharp;

namespace CollectManagement.Application.Interfaces.Services;

public interface IDocumentService
{
    Task<byte[]> GeneratePdfFromHtmlAsync(string html, bool isLandscape = false);

    Task<byte[]> GeneratePdfFromHtmlAsync(
        string html,
        PdfOptions? pdfOptions = null);

    Task<byte[]> GenerateCombinedPdfFromHtmlsAsync(
        IReadOnlyList<string> htmls,
        PdfOptions? pdfOptions = null);
    
    Task<byte[]> GenerateDocxFromHtmlAsync(
        string html, 
        bool isLandscape=false, 
        CancellationToken cancellationToken = default);

    byte[] GenerateExcel(string html);
}