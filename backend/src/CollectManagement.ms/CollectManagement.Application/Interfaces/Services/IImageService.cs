namespace CollectManagement.Application.Interfaces.Services;

public interface IImageService
{
    Task SaveImage(string image, string folderName, string imageName, CancellationToken cancellationToken);

    ValueTask<byte[]> GetImage(string folderName, string imageName, CancellationToken cancellationToken);
}