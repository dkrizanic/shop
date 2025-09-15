using Domain.Models;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ShoppingCartRepository : IShoppingCartRepository
{
    private readonly ApplicationDbContext _context;

    public ShoppingCartRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ShoppingCartItem>> GetUserCartItemsAsync(int userId)
    {
        return await _context.ShoppingCartItems
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync();
    }

    public async Task<ShoppingCartItem?> GetCartItemAsync(int userId, int productId)
    {
        return await _context.ShoppingCartItems
            .FirstOrDefaultAsync(item => item.UserId == userId && item.ProductId == productId);
    }

    public async Task<ShoppingCartItem> AddToCartAsync(int userId, int productId, int quantity)
    {
        var existingItem = await GetCartItemAsync(userId, productId);

        if (existingItem != null)
        {
            // Update quantity if item already exists
            existingItem.Quantity += quantity;
            existingItem.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existingItem;
        }

        var newItem = new ShoppingCartItem
        {
            UserId = userId,
            ProductId = productId,
            Quantity = quantity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ShoppingCartItems.Add(newItem);
        await _context.SaveChangesAsync();
        return newItem;
    }

    public async Task<ShoppingCartItem> UpdateCartItemQuantityAsync(int cartItemId, int quantity)
    {
        var cartItem = await _context.ShoppingCartItems.FindAsync(cartItemId);
        if (cartItem == null)
        {
            throw new ArgumentException($"Cart item with ID {cartItemId} not found");
        }

        cartItem.Quantity = quantity;
        cartItem.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return cartItem;
    }

    public async Task<bool> RemoveFromCartAsync(int cartItemId)
    {
        var cartItem = await _context.ShoppingCartItems.FindAsync(cartItemId);
        if (cartItem == null)
        {
            return false;
        }

        _context.ShoppingCartItems.Remove(cartItem);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveProductFromCartAsync(int userId, int productId)
    {
        var cartItem = await GetCartItemAsync(userId, productId);
        if (cartItem == null)
        {
            return false;
        }

        _context.ShoppingCartItems.Remove(cartItem);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task ClearUserCartAsync(int userId)
    {
        var cartItems = await _context.ShoppingCartItems
            .Where(item => item.UserId == userId)
            .ToListAsync();

        _context.ShoppingCartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCartItemCountAsync(int userId)
    {
        return await _context.ShoppingCartItems
            .Where(item => item.UserId == userId)
            .SumAsync(item => item.Quantity);
    }
}