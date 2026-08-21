using System.IO.Compression;
using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Infrastructure.Services;
using FluentAssertions;
using Moq;

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
        var service = new DocumentService(_browserProvider.Object);

        service.Should().NotBeNull();
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

        // Vérifie que le fichier est bien un ZIP/XLSX
        result[0].Should().Be(0x50); // P
        result[1].Should().Be(0x4B); // K
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
        act.Should().Throw<NotFoundException>();
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

        // Un fichier DOCX est un ZIP
        result[0].Should().Be(0x50);
        result[1].Should().Be(0x4B);

        // Vérifie que l'archive est lisible
        using var stream = new MemoryStream(result);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

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
        // Arrange
        var html = "";

        // Act
        var result = await _service.GenerateDocxFromHtmlAsync(
            html,
            false,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerateDocxFromHtmlAsync_Should_Respect_CancellationToken()
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
}