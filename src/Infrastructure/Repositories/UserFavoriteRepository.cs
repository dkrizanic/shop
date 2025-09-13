using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Domain.Repositories;
using Infrastructure.Data;
using DomainEntity = Domain.Entities;

namespace Infrastructure.Repositories
{
    public class UserFavoriteRepository : IUserFavoriteRepository
    {
        private readonly ApplicationDbContext _context;

        public UserFavoriteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserFavorite?> AddToFavoritesAsync(string userId, int productId)
        {
            // Check if already exists
            var existing = await _context.UserFavorites
                .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.ProductId == productId);

            if (existing != null)
            {
                return new UserFavorite
                {
                    Id = existing.Id,
                    UserId = existing.UserId,
                    ProductId = existing.ProductId,
                    CreatedAt = existing.CreatedAt
                };
            }

            var favoriteEntity = new DomainEntity.UserFavorite
            {
                UserId = userId,
                ProductId = productId
            };

            _context.UserFavorites.Add(favoriteEntity);
            await _context.SaveChangesAsync();

            // Convert to model
            return new UserFavorite
            {
                Id = favoriteEntity.Id,
                UserId = favoriteEntity.UserId,
                ProductId = favoriteEntity.ProductId,
                CreatedAt = favoriteEntity.CreatedAt
            };
        }

        public async Task<bool> RemoveFromFavoritesAsync(string userId, int productId)
        {
            var favorite = await _context.UserFavorites
                .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.ProductId == productId);

            if (favorite == null) return false;

            _context.UserFavorites.Remove(favorite);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsFavoriteAsync(string userId, int productId)
        {
            return await _context.UserFavorites
                .AnyAsync(uf => uf.UserId == userId && uf.ProductId == productId);
        }

        public async Task<IEnumerable<int>> GetUserFavoriteProductIdsAsync(string userId)
        {
            return await _context.UserFavorites
                .Where(uf => uf.UserId == userId)
                .Select(uf => uf.ProductId)
                .ToListAsync();
        }
    }
}