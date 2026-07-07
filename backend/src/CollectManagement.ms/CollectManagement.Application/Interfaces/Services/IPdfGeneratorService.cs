namespace CollectManagement.Application.Interfaces.Services;

public interface IPdfGeneratorService
{
    Task<byte[]> GeneratePdfFromHtml(string html);
}