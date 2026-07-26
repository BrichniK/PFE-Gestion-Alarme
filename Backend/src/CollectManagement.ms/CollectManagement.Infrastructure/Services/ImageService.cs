using System.IO.Abstractions;
using CollectManagement.Application.Interfaces.Services;

namespace CollectManagement.Infrastructure.Services;

public class ImageService : IImageService
{
    private readonly IFileSystem _fileSystem;
    private readonly string[] _initPath = new[] { "uploads", "images" };
    
    public ImageService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public async Task SaveImage(
        string image,
        string folderName,
        string imageName, 
        CancellationToken cancellationToken)
    {
        CreateFolderIfnotExists(folderName);
        var paths = new[] { _initPath[0], _initPath[1], folderName, imageName };
        var file = Convert.FromBase64String(image);
        using var stream = new MemoryStream(file);
        await _fileSystem.File
            .WriteAllBytesAsync(Path.Combine(paths), stream.ToArray(), cancellationToken)
            .ConfigureAwait(false);
    }

    public async ValueTask<byte[]> GetImage(
        string folderName, 
        string imageName,
        CancellationToken cancellationToken)
    {
        CreateFolderIfnotExists(folderName);
        var paths = new[] { _initPath[0], _initPath[1], folderName, imageName };
        if (!File.Exists(Path.Combine(paths)))
        {
            return [];
        }
        
        return await _fileSystem.File
            .ReadAllBytesAsync(Path.Combine(paths), cancellationToken)
            .ConfigureAwait(false);
    }



    private void CreateFolderIfnotExists(string folderName)
    {
        var paths = new[] { _initPath[0], _initPath[1], folderName };
        if (!Directory.Exists(Path.Combine(paths)))
        {
            Directory.CreateDirectory(Path.Combine(paths));
        }
    }
}