using BuildingBlock.Domain.Enums;

namespace BuildingBlock.Domain.SharedDto
{
    public class SearchParameters
    {
        public SearchParameters()
        {
        }

        public string? SearchText { get; set; }
        public OrderSort OrderSort { get; set; } = OrderSort.Newest;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public SearchParameters(string? searchText, OrderSort sortOrder, int page, int pageSize)
        {
            if (searchText != null)
                SearchText = searchText;
            OrderSort = sortOrder;
            PageNumber = page;
            PageSize = pageSize;
        }
    }
}