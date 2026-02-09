namespace MyPhotoBooth.Application.Interfaces;

public record ProcessedImageResult(
    Stream OriginalStream,
    Stream ThumbnailStream,
    int Width,
    int Height,
    string ExifDataJson
);

public interface IImageProcessingService
{
    Task<ProcessedImageResult> ProcessImageAsync(Stream imageStream, CancellationToken cancellationToken = default);
    bool IsValidImageFile(Stream stream, string contentType);
}
