using Microsoft.AspNetCore.Mvc;
using Domain.Repositories;

namespace Application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserFavoriteController : ControllerBase
    {
        private readonly IUserFavoriteRepository _favoriteRepository;

        public UserFavoriteController(IUserFavoriteRepository favoriteRepository)
        {
            _favoriteRepository = favoriteRepository;
        }

        [HttpPost]
        public async Task<IActionResult> AddToFavorites([FromBody] AddFavoriteRequest request)
        {
            try
            {
                var favorite = await _favoriteRepository.AddToFavoritesAsync(request.UserId, request.ProductId);
                return Ok(new { message = "Added to favorites", favorite });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to add favorite", error = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveFromFavorites([FromQuery] string userId, [FromQuery] int productId)
        {
            try
            {
                var removed = await _favoriteRepository.RemoveFromFavoritesAsync(userId, productId);
                if (removed)
                    return Ok(new { message = "Removed from favorites" });
                else
                    return NotFound(new { message = "Favorite not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to remove favorite", error = ex.Message });
            }
        }

        [HttpGet("check")]
        public async Task<IActionResult> IsFavorite([FromQuery] string userId, [FromQuery] int productId)
        {
            try
            {
                var isFavorite = await _favoriteRepository.IsFavoriteAsync(userId, productId);
                return Ok(new { userId, productId, isFavorite });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to check favorite", error = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserFavorites(string userId)
        {
            try
            {
                var favoriteIds = await _favoriteRepository.GetUserFavoriteProductIdsAsync(userId);
                return Ok(new { userId, favoriteProductIds = favoriteIds });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get user favorites", error = ex.Message });
            }
        }
    }

    public class AddFavoriteRequest
    {
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }
    }
}