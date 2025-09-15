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

        // Check if product is in stock
        if (!product.IsInStock)
        {
            throw new InvalidOperationException($"Product '{product.Title}' is currently out of stock");
        }

        // Check if requested quantity is available
        if (product.Stock < addToCartDto.Quantity)
        {
            throw new InvalidOperationException($"Only {product.Stock} items available for '{product.Title}'. Requested: {addToCartDto.Quantity}");
        }

        // Check existing cart item quantity + new quantity doesn't exceed stock
        var existingCartItem = await _cartRepository.GetCartItemAsync(userId, addToCartDto.ProductId);
        var totalRequestedQuantity = addToCartDto.Quantity + (existingCartItem?.Quantity ?? 0);

        if (product.Stock < totalRequestedQuantity)
        {
            var availableToAdd = product.Stock - (existingCartItem?.Quantity ?? 0);
            throw new InvalidOperationException($"Cannot add {addToCartDto.Quantity} items. Only {availableToAdd} more items can be added to cart for '{product.Title}'");
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

        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
        {
            throw new ArgumentException($"Product with ID {productId} not found");
        }

        // Validate stock availability for the updated quantity
        if (updateDto.Quantity > product.Stock)
        {
            throw new InvalidOperationException($"Cannot update quantity to {updateDto.Quantity}. Only {product.Stock} items available for '{product.Title}'");
        }

        var updatedItem = await _cartRepository.UpdateCartItemQuantityAsync(existingItem.Id, updateDto.Quantity);

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

    public async Task<CheckoutValidationResult> ValidateCartForCheckoutAsync(int userId)
    {
        var result = new CheckoutValidationResult { IsValid = true };

        // Get raw cart items directly from repository to include items with non-existent products
        var cartItems = await _cartRepository.GetUserCartItemsAsync(userId);

        if (!cartItems.Any())
        {
            result.IsValid = false;
            result.Message = "Cart is empty";
            return result;
        }

        var hasStockIssues = false;
        var updatedCartItems = new List<ShoppingCartItemDTO>();

        foreach (var cartItem in cartItems)
        {
            // Get fresh product data to check current stock
            var currentProduct = await _productService.GetProductByIdAsync(cartItem.ProductId);

            if (currentProduct == null)
            {
                // Product no longer exists
                result.Errors.Add(new CheckoutValidationError
                {
                    ProductId = cartItem.ProductId,
                    ProductTitle = "Unknown Product",
                    RequestedQuantity = cartItem.Quantity,
                    AvailableStock = 0,
                    AdjustedQuantity = 0,
                    ErrorType = CheckoutErrorType.ProductNotFound,
                    Message = $"Product with ID {cartItem.ProductId} is no longer available"
                });
                hasStockIssues = true;
                // Don't add to updated cart - effectively removing it
                continue;
            }

            if (currentProduct.Stock == 0)
            {
                // Product is now out of stock
                result.Errors.Add(new CheckoutValidationError
                {
                    ProductId = cartItem.ProductId,
                    ProductTitle = currentProduct.Title,
                    RequestedQuantity = cartItem.Quantity,
                    AvailableStock = 0,
                    AdjustedQuantity = 0,
                    ErrorType = CheckoutErrorType.OutOfStock,
                    Message = $"'{currentProduct.Title}' is now out of stock"
                });
                hasStockIssues = true;
                // Don't add to updated cart - effectively removing it
                continue;
            }

            if (currentProduct.Stock < cartItem.Quantity)
            {
                // Insufficient stock - adjust quantity to available stock
                var adjustedQuantity = currentProduct.Stock;
                result.Errors.Add(new CheckoutValidationError
                {
                    ProductId = cartItem.ProductId,
                    ProductTitle = currentProduct.Title,
                    RequestedQuantity = cartItem.Quantity,
                    AvailableStock = currentProduct.Stock,
                    AdjustedQuantity = adjustedQuantity,
                    ErrorType = CheckoutErrorType.InsufficientStock,
                    Message = $"Only {currentProduct.Stock} items available for '{currentProduct.Title}'. Quantity adjusted from {cartItem.Quantity} to {adjustedQuantity}"
                });
                hasStockIssues = true;

                // Add item with adjusted quantity
                updatedCartItems.Add(new ShoppingCartItemDTO
                {
                    Id = cartItem.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = adjustedQuantity,
                    CreatedAt = cartItem.CreatedAt,
                    UpdatedAt = DateTime.UtcNow,
                    Product = currentProduct
                });

                // Update the database with the adjusted quantity
                await _cartRepository.UpdateCartItemQuantityAsync(cartItem.Id, adjustedQuantity);
            }
            else
            {
                // Stock is sufficient - add item as is but with fresh product data
                updatedCartItems.Add(new ShoppingCartItemDTO
                {
                    Id = cartItem.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    CreatedAt = cartItem.CreatedAt,
                    UpdatedAt = cartItem.UpdatedAt,
                    Product = currentProduct
                });
            }
        }

        if (hasStockIssues)
        {
            result.IsValid = false;
            result.Message = result.Errors.Count == 1
                ? result.Errors[0].Message
                : $"Some items in your cart have stock issues. Please review the changes.";

            // Provide updated cart with current stock information
            result.UpdatedCart = new ShoppingCartDTO();
            result.UpdatedCart.Items.AddRange(updatedCartItems);
        }

        // Remove items that are no longer available (out of stock or not found)
        var itemsToRemove = result.Errors
            .Where(e => e.ErrorType == CheckoutErrorType.OutOfStock || e.ErrorType == CheckoutErrorType.ProductNotFound)
            .ToList();

        foreach (var item in itemsToRemove)
        {
            await _cartRepository.RemoveProductFromCartAsync(userId, item.ProductId);
        }

        return result;
    }
}