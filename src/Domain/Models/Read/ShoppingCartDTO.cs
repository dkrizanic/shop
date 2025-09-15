namespace Domain.Models.Read;

public class ShoppingCartDTO
{
    public List<ShoppingCartItemDTO> Items { get; set; } = new();
    public int TotalItems => Items.Sum(item => item.Quantity);
    public decimal TotalPrice => Items.Sum(item => item.TotalPrice);
    public DateTime LastUpdated => Items.Any() ? Items.Max(item => item.UpdatedAt) : DateTime.MinValue;
}