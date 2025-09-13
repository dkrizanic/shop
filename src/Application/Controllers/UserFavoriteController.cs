using Microsoft.AspNetCore.Mvc;
using Domain.Repositories;
using Domain.Validators;
using FluentValidation;

namespace Application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserFavoriteController : ControllerBase
    {
        private readonly IUserFavoriteRepository _favoriteRepository;
        private readonly IValidator<AddFavoriteRequest> _addFavoriteValidator;

        public UserFavoriteController(IUserFavoriteRepository favoriteRepository, IValidator<AddFavoriteRequest> addFavoriteValidator)
        {
            _favoriteRepository = favoriteRepository;
            _addFavoriteValidator = addFavoriteValidator;
        }

        [HttpPost]
        public async Task<IActionResult> AddToFavorites([FromBody] AddFavoriteRequest request)
        {
            var validationResult = await _addFavoriteValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var favorite = await _favoriteRepository.AddToFavoritesAsync(request.UserId, request.ProductId);
            return Ok(new { message = "Added to favorites", favorite });
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveFromFavorites([FromQuery] string userId, [FromQuery] int productId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID is required");

            if (productId <= 0)
                return BadRequest("Product ID must be greater than 0");

            var removed = await _favoriteRepository.RemoveFromFavoritesAsync(userId, productId);
            if (removed)
                return Ok(new { message = "Removed from favorites" });
            else
                return NotFound(new { message = "Favorite not found" });
        }

        [HttpGet("check")]
        public async Task<IActionResult> IsFavorite([FromQuery] string userId, [FromQuery] int productId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID is required");

            if (productId <= 0)
                return BadRequest("Product ID must be greater than 0");

            var isFavorite = await _favoriteRepository.IsFavoriteAsync(userId, productId);
            return Ok(new { userId, productId, isFavorite });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserFavorites(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID is required");

            var favoriteIds = await _favoriteRepository.GetUserFavoriteProductIdsAsync(userId);
            return Ok(new { userId, favoriteProductIds = favoriteIds });
        }
    }
}