namespace MyPhotoBooth.Application.Common.Pagination;

public class PaginatedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public PaginatedResult()
    {
    }

    public PaginatedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
    }

    public static PaginatedResult<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        return new PaginatedResult<T>(items, page, pageSize, totalCount);
    }

    public static PaginatedResult<T> Empty(int page, int pageSize)
    {
        return new PaginatedResult<T>(Array.Empty<T>(), page, pageSize, 0);
    }
}

public record PaginatedRequest(int Page = 1, int PageSize = 20)
{
    public int Skip => (Page - 1) * PageSize;
}
