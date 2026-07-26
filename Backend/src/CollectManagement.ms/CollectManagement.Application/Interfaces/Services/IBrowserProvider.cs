using PuppeteerSharp;

namespace CollectManagement.Application.Interfaces.Services;

public interface IBrowserProvider: IDisposable, IAsyncDisposable
{
    Task<IBrowser> GetBrowser();
}