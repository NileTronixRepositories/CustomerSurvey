namespace BuildingBlock.Domain.SharedDto
{
    public sealed record Pagination<T>
    {
        public int CurrentPage { get; }
        public int PageSize { get; }
        public int TotalPages { get; }
        public int TotalItems { get; }
        public IReadOnlyList<T> Data { get; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public Pagination(int currentPage, int pageSize, int totalItems, IReadOnlyList<T> data)
        {
            if (currentPage <= 0) throw new ArgumentException("currentPage must be greater than 0.");
            if (pageSize <= 0) throw new ArgumentException("pageSize must be greater than 0.");

            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalItems = totalItems;

            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);
            Data = data ?? new List<T>();
        }

        public override string ToString()
        {
            return $"Page {CurrentPage} of {TotalPages}, {TotalItems} items total, {Data.Count} items on this page.";
        }
    }
}