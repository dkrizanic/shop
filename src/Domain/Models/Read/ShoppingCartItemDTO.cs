namespace Domain.Models.Read;

public class ShoppingCartItemDTO
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Product information from external API
    public ProductDTO? Product { get; set; }

    // Calculated fields
    public decimal TotalPrice => Product?.Price * Quantity ?? 0;
}