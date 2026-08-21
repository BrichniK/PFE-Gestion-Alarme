using System.IO.Compression;
using System.Reflection;
using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Infrastructure.Services;
using FluentAssertions;
using Moq;
using PdfSharp.Pdf;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace CollectManagement.Tests.Infrastructure.Services;

public class DocumentServiceTests
{
    private readonly Mock<IBrowserProvider> _browserProvider;
    private readonly DocumentService _service;

    public DocumentServiceTests()
    {
        _browserProvider = new Mock<IBrowserProvider>();
        _service = new DocumentService(_browserProvider.Object);
    }

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    [Fact]
    public void Constructor_Should_Create_Service()
    {
        // Act
        var service = new DocumentService(_browserProvider.Object);

        // Assert
        service.Should().NotBeNull();
    }

    // ============================================================
    // PDF
    // ============================================================

    [Fact]
    public async Task GeneratePdfFromHtmlAsync_Should_Call_BrowserProvider()
    {
        // Arrange
        var browser = new Mock<IBrowser>();
        var page = new Mock<IPage>();

        var pdfBytes = CreatePdfBytes();

        _browserProvider
            .Setup(x => x.GetBrowser())
            .ReturnsAsync(browser.Object);

        browser
            .Setup(x => x.NewPageAsync())
            .ReturnsAsync(page.Object);

        // IMPORTANT :
        // SetContentAsync possède un argument optionnel.
        // On fournit donc explicitement le deuxième argument.
        page
            .Setup(x => x.SetContentAsync(
                It.IsAny<string>(),
                It.IsAny<NavigationOptions>()))
            .Returns(Task.CompletedTask);

        page
            .Setup(x => x.PdfDataAsync(It.IsAny<PdfOptions>()))
            .ReturnsAsync(pdfBytes);

        var result = await _service.GeneratePdfFromHtmlAsync(
            "<html><body><h1>Test</h1></body></html>",
            false);
        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().BeEquivalentTo(pdfBytes);

        _browserProvider.Verify(
            x => x.GetBrowser(),
            Times.Once);

        browser.Verify(
            x => x.NewPageAsync(),
            Times.Once);

        page.Verify(
            x => x.SetContentAsync(
                It.IsAny<string>(),
                It.IsAny<NavigationOptions>()),
            Times.Once);

        page.Verify(
            x => x.PdfDataAsync(It.IsAny<PdfOptions>()),
            Times.Once);
    }

    [Fact]
    public async Task GeneratePdfFromHtmlAsync_Should_Support_Landscape()
    {
        // Arrange
        var browser = new Mock<IBrowser>();
        var page = new Mock<IPage>();

        var pdfBytes = CreatePdfBytes();

        PdfOptions? capturedOptions = null;

        _browserProvider
            .Setup(x => x.GetBrowser())
            .ReturnsAsync(browser.Object);

        browser
            .Setup(x => x.NewPageAsync())
            .ReturnsAsync(page.Object);

        page
            .Setup(x => x.SetContentAsync(
                It.IsAny<string>(),
                It.IsAny<NavigationOptions>()))
            .Returns(Task.CompletedTask);

        page
            .Setup(x => x.PdfDataAsync(It.IsAny<PdfOptions>()))
            .Callback<PdfOptions>(options =>
            {
                capturedOptions = options;
            })
            .ReturnsAsync(pdfBytes);

        // Act
        var result = await _service.GeneratePdfFromHtmlAsync(
            "<html><body>Landscape</body></html>",
            true);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();

        capturedOptions.Should().NotBeNull();

        capturedOptions!.Landscape.Should().BeTrue();
        capturedOptions.Format.Should().Be(PaperFormat.A4);
        capturedOptions.PrintBackground.Should().BeTrue();
        capturedOptions.DisplayHeaderFooter.Should().BeFalse();
    }

    [Fact]
    public async Task GeneratePdfFromHtmlAsync_With_PdfOptions_Should_Use_Provided_Options()
    {
        // Arrange
        var browser = new Mock<IBrowser>();
        var page = new Mock<IPage>();

        var pdfBytes = CreatePdfBytes();

        var options = new PdfOptions
        {
            Format = PaperFormat.A4,
            Landscape = true,
            PrintBackground = true,
            DisplayHeaderFooter = false
        };

        PdfOptions? capturedOptions = null;

        _browserProvider
            .Setup(x => x.GetBrowser())
            .ReturnsAsync(browser.Object);

        browser
            .Setup(x => x.NewPageAsync())
            .ReturnsAsync(page.Object);

        page
            .Setup(x => x.SetContentAsync(
                It.IsAny<string>(),
                It.IsAny<NavigationOptions>()))
            .Returns(Task.CompletedTask);

        page
            .Setup(x => x.PdfDataAsync(It.IsAny<PdfOptions>()))
            .Callback<PdfOptions>(x =>
            {
                capturedOptions = x;
            })
            .ReturnsAsync(pdfBytes);

        // Act
        var result = await _service.GeneratePdfFromHtmlAsync(
            "<html><body>Test</body></html>",
            options);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();

        capturedOptions.Should().BeSameAs(options);
    }

    // ============================================================
    // COMBINED PDF
    // ============================================================

    [Fact]
    public async Task GenerateCombinedPdfFromHtmlsAsync_Should_Combine_Pdfs()
    {
        // Arrange
        var browser = new Mock<IBrowser>();

        var page1 = new Mock<IPage>();
        var page2 = new Mock<IPage>();

        var pdf1 = CreatePdfBytes();
        var pdf2 = CreatePdfBytes();

        _browserProvider
            .Setup(x => x.GetBrowser())
            .ReturnsAsync(browser.Object);

        browser
            .SetupSequence(x => x.NewPageAsync())
            .ReturnsAsync(page1.Object)
            .ReturnsAsync(page2.Object);

        page1
            .Setup(x => x.SetContentAsync(
                It.IsAny<string>(),
                It.IsAny<NavigationOptions>()))
            .Returns(Task.CompletedTask);

        page2
            .Setup(x => x.SetContentAsync(
                It.IsAny<string>(),
                It.IsAny<NavigationOptions>()))
            .Returns(Task.CompletedTask);

        page1
            .Setup(x => x.PdfDataAsync(It.IsAny<PdfOptions>()))
            .ReturnsAsync(pdf1);

        page2
            .Setup(x => x.PdfDataAsync(It.IsAny<PdfOptions>()))
            .ReturnsAsync(pdf2);

        // Act
        var result = await _service.GenerateCombinedPdfFromHtmlsAsync(
            new[]
            {
                "<html><body>Page 1</body></html>",
                "<html><body>Page 2</body></html>"
            });

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();

        // Vérifie la signature PDF
        result[0].Should().Be((byte)'%');
        result[1].Should().Be((byte)'P');
        result[2].Should().Be((byte)'D');
        result[3].Should().Be((byte)'F');

        browser.Verify(
            x => x.NewPageAsync(),
            Times.Exactly(2));

        page1.Verify(
            x => x.PdfDataAsync(It.IsAny<PdfOptions>()),
            Times.Once);

        page2.Verify(
            x => x.PdfDataAsync(It.IsAny<PdfOptions>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerateCombinedPdfFromHtmlsAsync_With_Empty_List_Should_Throw_InvalidOperationException()
    {
        // Arrange
        var browser = new Mock<IBrowser>();

        _browserProvider
            .Setup(x => x.GetBrowser())
            .ReturnsAsync(browser.Object);

        // Act
        Func<Task> act = async () =>
            await _service.GenerateCombinedPdfFromHtmlsAsync(
                Array.Empty<string>());

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot save a PDF document with no pages.");

        browser.Verify(
            x => x.NewPageAsync(),
            Times.Never);
    }
    // ============================================================
    // EXCEL
    // ============================================================

    [Fact]
    public void GenerateExcel_Should_Return_Valid_Excel_File()
    {
        // Arrange
        var html = """
                   <table class="excel-view">
                       <thead>
                           <tr>
                               <th>Nom</th>
                               <th>Temperature</th>
                               <th>Statut</th>
                           </tr>
                       </thead>
                       <tbody>
                           <tr>
                               <td>Machine 001</td>
                               <td>25.5</td>
                               <td>Normal</td>
                           </tr>
                       </tbody>
                   </table>
                   """;

        // Act
        var result = _service.GenerateExcel(html);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();

        // XLSX = ZIP
        result[0].Should().Be(0x50);
        result[1].Should().Be(0x4B);
    }

    [Fact]
    public void GenerateExcel_Should_Handle_Multiple_Tables()
    {
        // Arrange
        var html = """
                   <table class="excel-view">
                       <thead>
                           <tr>
                               <th>Machine</th>
                               <th>Temperature</th>
                           </tr>
                       </thead>
                       <tbody>
                           <tr>
                               <td>Machine 001</td>
                               <td>25.5</td>
                           </tr>
                       </tbody>
                   </table>

                   <table class="excel-view">
                       <thead>
                           <tr>
                               <th>Machine</th>
                               <th>Vibration</th>
                           </tr>
                       </thead>
                       <tbody>
                           <tr>
                               <td>Machine 002</td>
                               <td>3.25</td>
                           </tr>
                       </tbody>
                   </table>
                   """;

        // Act
        var result = _service.GenerateExcel(html);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateExcel_Should_Handle_Table_Without_Headers()
    {
        // Arrange
        var html = """
                   <table class="excel-view">
                       <tbody>
                           <tr>
                               <td>Machine 001</td>
                               <td>25.5</td>
                           </tr>
                       </tbody>
                   </table>
                   """;

        // Act
        var result = _service.GenerateExcel(html);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateExcel_Should_Handle_Table_Without_Rows()
    {
        // Arrange
        var html = """
                   <table class="excel-view">
                       <thead>
                           <tr>
                               <th>Machine</th>
                               <th>Temperature</th>
                           </tr>
                       </thead>
                   </table>
                   """;

        // Act
        var result = _service.GenerateExcel(html);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateExcel_Should_Handle_Text_And_Numeric_Values()
    {
        // Arrange
        var html = """
                   <table class="excel-view">
                       <thead>
                           <tr>
                               <th>Nom</th>
                               <th>Integer</th>
                               <th>Decimal</th>
                               <th>Negative</th>
                               <th>Text</th>
                           </tr>
                       </thead>
                       <tbody>
                           <tr>
                               <td>Machine</td>
                               <td>25</td>
                               <td>25.75</td>
                               <td>-10.5</td>
                               <td>Normal</td>
                           </tr>
                       </tbody>
                   </table>
                   """;

        // Act
        var result = _service.GenerateExcel(html);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateExcel_Should_Handle_Empty_Cell()
    {
        // Arrange
        var html = """
                   <table class="excel-view">
                       <thead>
                           <tr>
                               <th>Machine</th>
                               <th>Temperature</th>
                           </tr>
                       </thead>
                       <tbody>
                           <tr>
                               <td></td>
                               <td>25.5</td>
                           </tr>
                       </tbody>
                   </table>
                   """;

        // Act
        var result = _service.GenerateExcel(html);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateExcel_Should_Handle_Row_Without_Cells()
    {
        // Arrange
        var html = """
                   <table class="excel-view">
                       <tbody>
                           <tr></tr>
                           <tr>
                               <td>Machine 001</td>
                           </tr>
                       </tbody>
                   </table>
                   """;

        // Act
        var result = _service.GenerateExcel(html);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateExcel_Should_Handle_Integer_And_Decimal_Formats()
    {
        // Arrange
        var html = """
                   <table class="excel-view">
                       <tbody>
                           <tr>
                               <td>10</td>
                               <td>10.5</td>
                               <td>-25</td>
                               <td>-25.75</td>
                           </tr>
                       </tbody>
                   </table>
                   """;

        // Act
        var result = _service.GenerateExcel(html);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateExcel_Should_Throw_When_No_Excel_Table_Exists()
    {
        // Arrange
        var html = """
                   <html>
                       <body>
                           <table>
                               <tr>
                                   <td>Test</td>
                               </tr>
                           </table>
                       </body>
                   </html>
                   """;

        // Act
        Action act = () => _service.GenerateExcel(html);

        // Assert
        act.Should()
            .Throw<NotFoundException>()
            .WithMessage("Excel non pris en charge");
    }

    [Fact]
    public void GenerateExcel_Should_Throw_When_Html_Has_No_Table()
    {
        // Arrange
        var html = """
                   <html>
                       <body>
                           <h1>Rapport maintenance</h1>
                           <p>Aucune donnée</p>
                       </body>
                   </html>
                   """;

        // Act
        Action act = () => _service.GenerateExcel(html);

        // Assert
        act.Should()
            .Throw<NotFoundException>();
    }

    // ============================================================
    // DOCX
    // ============================================================

    [Fact]
    public async Task GenerateDocxFromHtmlAsync_Should_Return_Docx_File()
    {
        // Arrange
        var html = """
                   <html>
                       <body>
                           <h1>Rapport de maintenance</h1>
                           <p>Machine 001</p>
                           <p>Température : 25.5</p>
                       </body>
                   </html>
                   """;

        // Act
        var result = await _service.GenerateDocxFromHtmlAsync(
            html,
            false,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();

        // DOCX = ZIP
        result[0].Should().Be(0x50);
        result[1].Should().Be(0x4B);

        using var stream = new MemoryStream(result);
        using var archive = new ZipArchive(
            stream,
            ZipArchiveMode.Read);

        archive.Entries.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerateDocxFromHtmlAsync_Should_Handle_Landscape_Mode()
    {
        // Arrange
        var html = """
                   <html>
                       <body>
                           <h1>Rapport paysage</h1>
                           <table>
                               <tr>
                                   <td>Machine</td>
                                   <td>Temperature</td>
                               </tr>
                               <tr>
                                   <td>Machine 001</td>
                                   <td>25.5</td>
                               </tr>
                           </table>
                       </body>
                   </html>
                   """;

        // Act
        var result = await _service.GenerateDocxFromHtmlAsync(
            html,
            true,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();

        result[0].Should().Be(0x50);
        result[1].Should().Be(0x4B);
    }

    [Fact]
    public async Task GenerateDocxFromHtmlAsync_Should_Handle_Empty_Html()
    {
        // Act
        var result = await _service.GenerateDocxFromHtmlAsync(
            "",
            false,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerateDocxFromHtmlAsync_Should_Handle_CancellationToken()
    {
        // Arrange
        var html = """
                   <html>
                       <body>
                           <p>Test</p>
                       </body>
                   </html>
                   """;

        using var cts = new CancellationTokenSource();

        // Act
        var result = await _service.GenerateDocxFromHtmlAsync(
            html,
            false,
            cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    // ============================================================
    // PRIVATE HELPERS
    // ============================================================




    [Theory]
    [InlineData("10")]
    [InlineData("10.5")]
    [InlineData("-10")]
    [InlineData("-10.5")]
    [InlineData("0")]
    [InlineData("0.25")]
    public void IsNumeric_Should_Return_True_For_Numeric_Value(
        string value)
    {
        // Arrange
        var method = typeof(DocumentService)
            .GetMethod(
                "IsNumeric",
                BindingFlags.NonPublic | BindingFlags.Static);

        method.Should().NotBeNull();

        // Act
        var result = method!.Invoke(
            null,
            new object[] { value });

        // Assert
        result.Should().Be(true);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("Machine 001")]
    [InlineData("")]
    public void IsNumeric_Should_Return_False_For_Non_Numeric_Value(
        string value)
    {
        // Arrange
        var method = typeof(DocumentService)
            .GetMethod(
                "IsNumeric",
                BindingFlags.NonPublic | BindingFlags.Static);

        method.Should().NotBeNull();

        // Act
        var result = method!.Invoke(
            null,
            new object[] { value });

        // Assert
        result.Should().Be(false);
    }

    // ============================================================
    // HELPER
    // ============================================================

    private static byte[] CreatePdfBytes()
    {
        using var stream = new MemoryStream();

        using (var document = new PdfDocument())
        {
            var page = document.AddPage();

            page.Width = 595;
            page.Height = 842;

            document.Save(stream);
        }

        return stream.ToArray();
    }
}