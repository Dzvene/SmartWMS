namespace SmartWMS.API.Common.Models;

/// <summary>
/// Paginated response wrapper matching SmartWMS_UI pagination contract
/// </summary>
public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }

    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public static PaginatedResponse<T> Create(List<T> items, int page, int pageSize, int totalCount)
    {
        return new PaginatedResponse<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}

/// <summary>
/// Query parameters for paginated requests
/// </summary>
public class QueryParams
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 25;

    private int _page = 1;
    private int _pageSize = DefaultPageSize;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? DefaultPageSize : value;
    }

    public string? SortBy { get; set; }
    public string SortOrder { get; set; } = "asc";
    public string? Search { get; set; }
}
