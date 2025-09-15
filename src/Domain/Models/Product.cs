namespace Domain.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Rating { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Thumbnail { get; set; } = string.Empty;
        public int Stock { get; set; } = 0;
    }
}