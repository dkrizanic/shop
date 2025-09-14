using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Shop.UnitTests.Repositories
{
    public class UserFavoriteRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UserFavoriteRepository _repository;

        public UserFavoriteRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new UserFavoriteRepository(_context);
        }

        [Fact]
        public async Task AddToFavoritesAsync_NewFavorite_ShouldAddSuccessfully()
        {
            // Arrange
            var userId = "test-user";
            var productId = 1;

            // Act
            var result = await _repository.AddToFavoritesAsync(userId, productId);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.ProductId.Should().Be(productId);
            result.Id.Should().BeGreaterThan(0);

            var favoriteInDb = await _context.UserFavorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);
            favoriteInDb.Should().NotBeNull();
        }

        [Fact]
        public async Task AddToFavoritesAsync_DuplicateFavorite_ShouldReturnExisting()
        {
            // Arrange
            var userId = "test-user";
            var productId = 1;

            // Add first favorite
            var first = await _repository.AddToFavoritesAsync(userId, productId);

            // Act - Try to add same favorite again
            var second = await _repository.AddToFavoritesAsync(userId, productId);

            // Assert
            second.Should().NotBeNull();
            second!.Id.Should().Be(first!.Id);
            second.UserId.Should().Be(userId);
            second.ProductId.Should().Be(productId);
        }

        [Fact]
        public async Task RemoveFromFavoritesAsync_ExistingFavorite_ShouldReturnTrue()
        {
            // Arrange
            var userId = "test-user";
            var productId = 1;
            await _repository.AddToFavoritesAsync(userId, productId);

            // Act
            var result = await _repository.RemoveFromFavoritesAsync(userId, productId);

            // Assert
            result.Should().BeTrue();

            var favoriteInDb = await _context.UserFavorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);
            favoriteInDb.Should().BeNull();
        }

        [Fact]
        public async Task RemoveFromFavoritesAsync_NonExistingFavorite_ShouldReturnFalse()
        {
            // Arrange
            var userId = "test-user";
            var productId = 999;

            // Act
            var result = await _repository.RemoveFromFavoritesAsync(userId, productId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsFavoriteAsync_ExistingFavorite_ShouldReturnTrue()
        {
            // Arrange
            var userId = "test-user";
            var productId = 1;
            await _repository.AddToFavoritesAsync(userId, productId);

            // Act
            var result = await _repository.IsFavoriteAsync(userId, productId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsFavoriteAsync_NonExistingFavorite_ShouldReturnFalse()
        {
            // Arrange
            var userId = "test-user";
            var productId = 999;

            // Act
            var result = await _repository.IsFavoriteAsync(userId, productId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetUserFavoriteProductIdsAsync_WithFavorites_ShouldReturnIds()
        {
            // Arrange
            var userId = "test-user";
            var productIds = new[] { 1, 2, 3 };

            foreach (var productId in productIds)
            {
                await _repository.AddToFavoritesAsync(userId, productId);
            }

            // Act
            var result = await _repository.GetUserFavoriteProductIdsAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(productIds);
        }

        [Fact]
        public async Task GetUserFavoriteProductIdsAsync_NoFavorites_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = "test-user";

            // Act
            var result = await _repository.GetUserFavoriteProductIdsAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUserFavoriteProductIdsAsync_DifferentUsers_ShouldReturnCorrectIds()
        {
            // Arrange
            var userId1 = "user1";
            var userId2 = "user2";

            await _repository.AddToFavoritesAsync(userId1, 1);
            await _repository.AddToFavoritesAsync(userId1, 2);
            await _repository.AddToFavoritesAsync(userId2, 3);
            await _repository.AddToFavoritesAsync(userId2, 4);

            // Act
            var result1 = await _repository.GetUserFavoriteProductIdsAsync(userId1);
            var result2 = await _repository.GetUserFavoriteProductIdsAsync(userId2);

            // Assert
            result1.Should().BeEquivalentTo(new[] { 1, 2 });
            result2.Should().BeEquivalentTo(new[] { 3, 4 });
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}