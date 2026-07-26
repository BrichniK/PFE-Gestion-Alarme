using System.Globalization;
using ClosedXML.Excel;
using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Interfaces.Services;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlAgilityPack;
using HtmlToOpenXml;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace CollectManagement.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private readonly IBrowserProvider _browserProvider;

    public DocumentService(
        IBrowserProvider browserProvider)
    {
        _browserProvider = browserProvider;
    }
    
#pragma warning disable CA2007
    public async Task<byte[]> GeneratePdfFromHtmlAsync(string html, bool isLandscape = false)
    {
        var browser = await _browserProvider.GetBrowser();

        return await GenerateSinglePdfAsync(browser, html, new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            DisplayHeaderFooter = false,
            Landscape = isLandscape // Set landscape or portrait based on the parameter
        });
    }
    
    public async Task<byte[]> GeneratePdfFromHtmlAsync(
        string html,
        PdfOptions? pdfOptions = null)
    {
        var browser = await _browserProvider.GetBrowser();

        return await GenerateSinglePdfAsync(browser, html, pdfOptions);
    }
    
    public async Task<byte[]> GenerateCombinedPdfFromHtmlsAsync(
        IReadOnlyList<string> htmls,
        PdfOptions? pdfOptions = null)
    {
        var browser = await _browserProvider.GetBrowser();
        var outputStream = new MemoryStream();
        using var finalPdf = new PdfDocument();
        
        foreach (var html in htmls)
        {
            var pdfStream = await GenerateSinglePdfAsync(browser, html, pdfOptions);
            using var stream = new MemoryStream(pdfStream);
            using var pdf = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
            
            for (int i = 0; i < pdf.PageCount; i++)
            {
                finalPdf.AddPage(pdf.Pages[i]);
            }
        }
        
        await finalPdf.SaveAsync(outputStream);
        return outputStream.ToArray();
    }

    private static async Task<byte[]> GenerateSinglePdfAsync(IBrowser browser, string html, PdfOptions? pdfOptions = null)
    {
        await using var page = await browser.NewPageAsync();
                    
        // Set the HTML content to the page
        await page.SetContentAsync(html);
    
        // Generate PDF
        return await page.PdfDataAsync(pdfOptions ?? new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            DisplayHeaderFooter = false,
            Landscape = false
        });
    }
    
    public async Task<byte[]> GenerateDocxFromHtmlAsync(
        string html, 
        bool isLandscape=false, 
        CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        using (var wordDoc = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
        {
            var mainPart = wordDoc.AddMainDocumentPart();
            
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
            
            var htmlConverter = new HtmlConverter(mainPart);
            mainPart.Document.Body = new Body();

            if (isLandscape)
            {
                var sectionProperties = new SectionProperties();
                var size = new PageSize();
                size.Orient = PageOrientationValues.Landscape;
                sectionProperties.AppendChild(size);
                mainPart.Document.Body.AppendChild(sectionProperties);
            }

            await htmlConverter.ParseBody(html, cancellationToken)
                    .ConfigureAwait(false);

            mainPart.Document.Save();
        }

        await memoryStream.FlushAsync(cancellationToken).ConfigureAwait(false);
        return memoryStream.ToArray();
    }
    
    public byte[] GenerateExcel(string html)
    {
        // Parse HTML to find the table
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);
    
        var tables = htmlDoc.DocumentNode.SelectNodes("//table[contains(@class, 'excel-view')]") 
                    ?? throw new NotFoundException("Excel non pris en charge");
        
        // Convert to Excel
        return HtmlTableToExcel(tables.Select(t => t.OuterHtml).ToList());
    }
    
    private static byte[] HtmlTableToExcel(List<string> tableHtmls)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sheet 1");
            
        int currentRow = 1; // Track current row position
    
        foreach (var tableHtml in tableHtmls)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(tableHtml);
        
            // Process headers
            var headers = htmlDoc.DocumentNode.SelectNodes("//th");
            if (headers != null)
            {
                for (int i = 0; i < headers.Count; i++)
                {
                    worksheet.Cell(currentRow, i + 1).Value = headers[i].InnerText.Trim();
                    worksheet.Cell(currentRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(currentRow, i + 1).Style.Fill.BackgroundColor = XLColor.GhostWhite;
                    worksheet.Cell(currentRow, i + 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                worksheet.Row(currentRow).Height = 22;
                currentRow++;
            }
        
            // Process rows
            var rows = htmlDoc.DocumentNode.SelectNodes("//tbody/tr");
            if (rows != null)
            {
                foreach (var row in rows)
                {
                    var cells = row.SelectNodes(".//td");
                    if (cells == null) continue;
                    for (int c = 0; c < cells.Count; c++)
                    {
                        var cellValue = cells[c].InnerText.Trim();
                        var excelCell = worksheet.Cell(currentRow, c + 1);
                        if (IsNumeric(cellValue))
                        {
                            excelCell.Value = Convert.ToDouble(cellValue, CultureInfo.InvariantCulture);
                            excelCell.Style.NumberFormat.Format = GetNumberFormat(cellValue);
                        }
                        else
                        {
                            excelCell.Value = cellValue;
                        }
                        excelCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        excelCell.Style.Fill.BackgroundColor = XLColor.White;
                        excelCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }
                    currentRow++;
                }
            }
        
            // Add spacing between tables (2 empty rows)
            currentRow += 2;
        }
    
        worksheet.Columns().AdjustToContents();
        worksheet.Rows().AdjustToContents();
        worksheet.RowsUsed().Height = 20;
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
    
    private static bool IsNumeric(string value)
    {
        return double.TryParse(value, 
            NumberStyles.Any, 
            CultureInfo.InvariantCulture, 
            out _);
    }

    private static string GetNumberFormat(string numericValue)
    {
        // Detect if it's an integer
        if (int.TryParse(numericValue, out _))
            return "0"; // Integer format
    
        // Detect if it has decimal places
        if (!numericValue.Contains('.', StringComparison.InvariantCulture) && 
            !numericValue.Contains(',', StringComparison.InvariantCulture)) return "0.000";
        
        // Count decimal places
        var decimalSeparator = CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator;
        var parts = numericValue.Split([decimalSeparator], StringSplitOptions.None);

        if (parts.Length != 3) return "0.000";
        int decimalPlaces = parts[1].Length;
        return $"0.{new string('0', decimalPlaces)}";

    }
}
