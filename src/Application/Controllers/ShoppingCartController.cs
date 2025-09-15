using Domain.Models.Read;
using Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Application.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShoppingCartController : ControllerBase
{
    private readonly IShoppingCartService _cartService;

    public ShoppingCartController(IShoppingCartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<ShoppingCartDTO>> GetCart()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        try
        {
            var userCart = await _cartService.GetUserCartAsync(userId.Value);
            return Ok(userCart);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to retrieve cart", error = ex.Message });
        }
    }

    [HttpPost("items")]
    public async Task<ActionResult<ShoppingCartItemDTO>> AddToCart([FromBody] AddToCartDTO addToCartDto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        try
        {
            var cartItemResponse = await _cartService.AddToCartAsync(userId.Value, addToCartDto);
            return Ok(cartItemResponse);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to add item to cart", error = ex.Message });
        }
    }

    [HttpPut("items/{productId}")]
    public async Task<ActionResult<ShoppingCartItemDTO>> UpdateCartItem(int productId, [FromBody] UpdateCartItemDTO updateDto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (productId <= 0)
            return BadRequest("Product ID must be greater than 0");

        try
        {
            var updatedCartItem = await _cartService.UpdateCartItemQuantityAsync(userId.Value, productId, updateDto);
            return Ok(updatedCartItem);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to update cart item", error = ex.Message });
        }
    }

    [HttpDelete("items/{productId}")]
    public async Task<ActionResult> RemoveFromCart(int productId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (productId <= 0)
            return BadRequest("Product ID must be greater than 0");

        try
        {
            var wasRemoved = await _cartService.RemoveFromCartAsync(userId.Value, productId);
            if (!wasRemoved)
                return NotFound(new { message = "Cart item not found" });

            return Ok(new { message = "Item removed from cart successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to remove item from cart", error = ex.Message });
        }
    }

    [HttpDelete]
    public async Task<ActionResult> ClearCart()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        try
        {
            await _cartService.ClearCartAsync(userId.Value);
            return Ok(new { message = "Cart cleared successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to clear cart", error = ex.Message });
        }
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetCartItemCount()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        try
        {
            var itemCount = await _cartService.GetCartItemCountAsync(userId.Value);
            return Ok(new { count = itemCount });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to get cart item count", error = ex.Message });
        }
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim != null && int.TryParse(userIdClaim, out int userId))
        {
            return userId;
        }
        return null;
    }
}