namespace AutoRent.Backend.Shared.Pagination;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    long TotalCount)
{
    public int TotalPages => PageSize <= 0
        ? 0
        : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;

    public static PagedResult<T> Create(
        IReadOnlyList<T> items,
        PaginationParams pagination,
        long totalCount)
    {
        var normalized = pagination.Normalize();
        return new PagedResult<T>(items, normalized.Page, normalized.PageSize, totalCount);
    }
}
