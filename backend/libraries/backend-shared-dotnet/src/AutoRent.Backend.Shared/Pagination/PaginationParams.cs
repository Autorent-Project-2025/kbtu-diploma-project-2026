namespace AutoRent.Backend.Shared.Pagination;

public sealed record PaginationParams
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int DefaultMaxPageSize = 100;

    public int Page { get; init; } = DefaultPage;
    public int PageSize { get; init; } = DefaultPageSize;

    public int Skip => (Page - 1) * PageSize;

    public PaginationParams Normalize(int maxPageSize = DefaultMaxPageSize)
    {
        var normalizedMaxPageSize = Math.Max(1, maxPageSize);
        return this with
        {
            Page = Math.Max(DefaultPage, Page),
            PageSize = Math.Clamp(PageSize, 1, normalizedMaxPageSize)
        };
    }
}
