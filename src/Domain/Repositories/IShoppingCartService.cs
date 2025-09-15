using Domain.Models.Read;

namespace Domain.Repositories;

public interface IShoppingCartService
{
    Task<ShoppingCartDTO> GetUserCartAsync(int userId);
    Task<ShoppingCartItemDTO> AddToCartAsync(int userId, AddToCartDTO addToCartDto);
    Task<ShoppingCartItemDTO> UpdateCartItemQuantityAsync(int userId, int productId, UpdateCartItemDTO updateDto);
    Task<bool> RemoveFromCartAsync(int userId, int productId);
    Task ClearCartAsync(int userId);
    Task<int> GetCartItemCountAsync(int userId);
    Task<CheckoutValidationResult> ValidateCartForCheckoutAsync(int userId);
}