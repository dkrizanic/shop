using System.Text.Json.Serialization;

namespace Domain.Models
{
    public class ProductQueryParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        [JsonPropertyName("search")]
        public string? SearchTerm { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("minPrice")]
        public decimal? MinPrice { get; set; }

        [JsonPropertyName("maxPrice")]
        public decimal? MaxPrice { get; set; }

        [JsonPropertyName("sortBy")]
        public string? SortBy { get; set; }

        [JsonPropertyName("sortOrder")]
        public string? SortOrder { get; set; } = "asc";
    }
}