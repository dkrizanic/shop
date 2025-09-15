using Domain.Models;
using Domain.Models.Read;
using Domain.Repositories;

namespace Infrastructure.Services;

public class ShoppingCartService : IShoppingCartService
{
    private readonly IShoppingCartRepository _cartRepository;
    private readonly IProductService _productService;

    public ShoppingCartService(IShoppingCartRepository cartRepository, IProductService productService)
    {
        _cartRepository = cartRepository;
        _productService = productService;
    }

    public async Task<ShoppingCartDTO> GetUserCartAsync(int userId)
    {
        var cartItems = await _cartRepository.GetUserCartItemsAsync(userId);
        var cartDto = new ShoppingCartDTO();

        foreach (var item in cartItems)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            if (product != null)
            {
                cartDto.Items.Add(new ShoppingCartItemDTO
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt,
                    Product = product
                });
            }
        }

        return cartDto;
    }

    public async Task<ShoppingCartItemDTO> AddToCartAsync(int userId, AddToCartDTO addToCartDto)
    {
        // Verify product exists
        var product = await _productService.GetProductByIdAsync(addToCartDto.ProductId);
        if (product == null)
        {
            throw new ArgumentException($"Product with ID {addToCartDto.ProductId} not found");
        }

        var cartItem = await _cartRepository.AddToCartAsync(userId, addToCartDto.ProductId, addToCartDto.Quantity);

        return new ShoppingCartItemDTO
        {
            Id = cartItem.Id,
            ProductId = cartItem.ProductId,
            Quantity = cartItem.Quantity,
            CreatedAt = cartItem.CreatedAt,
            UpdatedAt = cartItem.UpdatedAt,
            Product = product
        };
    }

    public async Task<ShoppingCartItemDTO> UpdateCartItemQuantityAsync(int userId, int productId, UpdateCartItemDTO updateDto)
    {
        var existingItem = await _cartRepository.GetCartItemAsync(userId, productId);
        if (existingItem == null)
        {
            throw new ArgumentException($"Cart item for product {productId} not found for user {userId}");
        }

        var updatedItem = await _cartRepository.UpdateCartItemQuantityAsync(existingItem.Id, updateDto.Quantity);
        var product = await _productService.GetProductByIdAsync(productId);

        return new ShoppingCartItemDTO
        {
            Id = updatedItem.Id,
            ProductId = updatedItem.ProductId,
            Quantity = updatedItem.Quantity,
            CreatedAt = updatedItem.CreatedAt,
            UpdatedAt = updatedItem.UpdatedAt,
            Product = product
        };
    }

    public async Task<bool> RemoveFromCartAsync(int userId, int productId)
    {
        return await _cartRepository.RemoveProductFromCartAsync(userId, productId);
    }

    public async Task ClearCartAsync(int userId)
    {
        await _cartRepository.ClearUserCartAsync(userId);
    }

    public async Task<int> GetCartItemCountAsync(int userId)
    {
        return await _cartRepository.GetCartItemCountAsync(userId);
    }
}