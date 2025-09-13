namespace Domain.Models.Read
{
    public class DummyJsonProductsResponse
    {
        public List<Product> Products { get; set; } = new();
        public int Total { get; set; }
        public int Skip { get; set; }
        public int Limit { get; set; }
    }
}