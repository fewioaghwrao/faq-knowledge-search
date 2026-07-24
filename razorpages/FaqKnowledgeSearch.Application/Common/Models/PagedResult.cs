namespace FaqKnowledgeSearch.Application.Common.Models;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages =>
        TotalCount == 0
            ? 0
            : (int)Math.Ceiling(
                TotalCount / (double)PageSize);

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;
}