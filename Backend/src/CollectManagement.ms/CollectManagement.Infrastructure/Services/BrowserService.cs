using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Infrastructure.PuppeteerConfig;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace CollectManagement.Infrastructure.Services;

public class BrowserProvider : IBrowserProvider
{
    private readonly PuppeteerOptions _puppeteerOptions;
    private IBrowser? _browser;

    public BrowserProvider(
        IOptions<PuppeteerOptions> puppeteerOptions)
    {
        ArgumentNullException.ThrowIfNull(puppeteerOptions);
        
        _puppeteerOptions = puppeteerOptions.Value;
    }

    public async Task<IBrowser> GetBrowser()
    {
        var launchOptions = new LaunchOptions
        {
            Headless = true
        };

        if (!string.IsNullOrEmpty(_puppeteerOptions.ChromePath))
        {
            launchOptions.ExecutablePath = _puppeteerOptions.ChromePath;
        }
        
        _browser ??= await Puppeteer.LaunchAsync(launchOptions).ConfigureAwait(false);

        if (_browser.IsClosed)
        {
            _browser = await Puppeteer.LaunchAsync(launchOptions).ConfigureAwait(false);
        }
        
        return _browser;
    }
    
    private bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _browser?.Dispose();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_disposed)
        {
            return;
        }

        if (_browser != null)
        {
            await _browser.DisposeAsync().ConfigureAwait(false);
        }

        _disposed = true;
    }
    
    public async ValueTask DisposeAsync() { 
        await DisposeAsyncCore().ConfigureAwait(false); 

        GC.SuppressFinalize(this); 
    }
}