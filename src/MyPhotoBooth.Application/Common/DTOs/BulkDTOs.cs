namespace MyPhotoBooth.Application.Common.DTOs;

public class BulkOperationResultDto
{
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<BulkOperationErrorDto> Errors { get; set; } = new();
}

public class BulkOperationErrorDto
{
    public string PhotoId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

public class BulkOperationRequestDto
{
    public List<Guid> PhotoIds { get; set; } = new();
}

public class BulkToggleFavoriteRequestDto : BulkOperationRequestDto
{
    public bool Favorite { get; set; }
}

public class BulkAlbumOperationRequestDto : BulkOperationRequestDto
{
    public Guid AlbumId { get; set; }
}

public class BulkDownloadResultDto
{
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public byte[] FileContents { get; set; } = Array.Empty<byte>();
    public int PhotoCount { get; set; }
    public long FileSize { get; set; }
}
