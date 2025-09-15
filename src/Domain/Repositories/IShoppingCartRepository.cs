using Domain.Models;

namespace Domain.Repositories;

public interface IShoppingCartRepository
{
    Task<List<ShoppingCartItem>> GetUserCartItemsAsync(int userId);
    Task<ShoppingCartItem?> GetCartItemAsync(int userId, int productId);
    Task<ShoppingCartItem> AddToCartAsync(int userId, int productId, int quantity);
    Task<ShoppingCartItem> UpdateCartItemQuantityAsync(int cartItemId, int quantity);
    Task<bool> RemoveFromCartAsync(int cartItemId);
    Task<bool> RemoveProductFromCartAsync(int userId, int productId);
    Task ClearUserCartAsync(int userId);
    Task<int> GetCartItemCountAsync(int userId);
}