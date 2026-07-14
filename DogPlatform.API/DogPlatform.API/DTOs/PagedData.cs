namespace DogPlatform.API.DTOs;

public sealed class PagedData<T>(
    IEnumerable<T> items, 
    int totalCount, 
    int page,
    int pageSize)
{
    public IEnumerable<T> Items { get; } = items;

    public int TotalCount { get; } = totalCount;

    public int Page { get; } = page;

    public int PageSize { get; } = pageSize;

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}