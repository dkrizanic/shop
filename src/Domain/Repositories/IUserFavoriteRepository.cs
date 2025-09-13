using Domain.Models;

namespace Domain.Repositories
{
    public interface IUserFavoriteRepository
    {
        Task<UserFavorite?> AddToFavoritesAsync(string userId, int productId);
        Task<bool> RemoveFromFavoritesAsync(string userId, int productId);
        Task<bool> IsFavoriteAsync(string userId, int productId);
        Task<IEnumerable<int>> GetUserFavoriteProductIdsAsync(string userId);
    }
}