using Domain.Models;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Shop.UnitTests.Repositories
{
    public class ShoppingCartRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ShoppingCartRepository _repository;

        public ShoppingCartRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new ShoppingCartRepository(_context);
        }

        [Fact]
        public async Task GetUserCartItemsAsync_WithItems_ShouldReturnUserCartItems()
        {
            // Arrange
            var userId = 1;
            var user = new User { Id = userId, Email = "test@example.com", PasswordHash = "hash", FirstName = "Test", LastName = "User" };
            await _context.Users.AddAsync(user);

            var cartItems = new List<ShoppingCartItem>
            {
                new ShoppingCartItem { UserId = userId, ProductId = 10, Quantity = 2, CreatedAt = DateTime.UtcNow.AddHours(-2), UpdatedAt = DateTime.UtcNow.AddHours(-1) },
                new ShoppingCartItem { UserId = userId, ProductId = 20, Quantity = 1, CreatedAt = DateTime.UtcNow.AddHours(-1), UpdatedAt = DateTime.UtcNow },
                new ShoppingCartItem { UserId = 2, ProductId = 30, Quantity = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };

            await _context.ShoppingCartItems.AddRangeAsync(cartItems);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserCartItemsAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(item => item.UserId.Should().Be(userId));
            result.Should().BeInAscendingOrder(x => x.CreatedAt);
        }

        [Fact]
        public async Task GetUserCartItemsAsync_WithEmptyCart_ShouldReturnEmpty()
        {
            // Arrange
            var userId = 1;

            // Act
            var result = await _repository.GetUserCartItemsAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCartItemAsync_WithExistingItem_ShouldReturnItem()
        {
            // Arrange
            var userId = 1;
            var productId = 10;
            var user = new User { Id = userId, Email = "test@example.com", PasswordHash = "hash", FirstName = "Test", LastName = "User" };
            await _context.Users.AddAsync(user);

            var cartItem = new ShoppingCartItem { UserId = userId, ProductId = productId, Quantity = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            await _context.ShoppingCartItems.AddAsync(cartItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCartItemAsync(userId, productId);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(userId);
            result.ProductId.Should().Be(productId);
            result.Quantity.Should().Be(2);
        }

        [Fact]
        public async Task GetCartItemAsync_WithNonExistentItem_ShouldReturnNull()
        {
            // Arrange
            var userId = 1;
            var productId = 999;

            // Act
            var result = await _repository.GetCartItemAsync(userId, productId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddToCartAsync_WithNewItem_ShouldCreateNewItem()
        {
            // Arrange
            var userId = 1;
            var productId = 10;
            var quantity = 3;
            var user = new User { Id = userId, Email = "test@example.com", PasswordHash = "hash", FirstName = "Test", LastName = "User" };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.AddToCartAsync(userId, productId, quantity);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.ProductId.Should().Be(productId);
            result.Quantity.Should().Be(quantity);
            result.Id.Should().BeGreaterThan(0);

            var itemInDb = await _context.ShoppingCartItems.FirstOrDefaultAsync(x => x.Id == result.Id);
            itemInDb.Should().NotBeNull();
        }

        [Fact]
        public async Task AddToCartAsync_WithExistingItem_ShouldUpdateQuantity()
        {
            // Arrange
            var userId = 1;
            var productId = 10;
            var initialQuantity = 2;
            var additionalQuantity = 3;
            var user = new User { Id = userId, Email = "test@example.com", PasswordHash = "hash", FirstName = "Test", LastName = "User" };
            await _context.Users.AddAsync(user);

            var originalTime = DateTime.UtcNow.AddHours(-1);
            var existingItem = new ShoppingCartItem
            {
                UserId = userId,
                ProductId = productId,
                Quantity = initialQuantity,
                CreatedAt = originalTime,
                UpdatedAt = originalTime
            };
            await _context.ShoppingCartItems.AddAsync(existingItem);
            await _context.SaveChangesAsync();

            // Add a small delay to ensure time difference
            await Task.Delay(10);

            // Act
            var result = await _repository.AddToCartAsync(userId, productId, additionalQuantity);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(existingItem.Id);
            result.Quantity.Should().Be(initialQuantity + additionalQuantity);
            result.UpdatedAt.Should().BeAfter(originalTime);

            var itemInDb = await _context.ShoppingCartItems.FirstOrDefaultAsync(x => x.Id == result.Id);
            itemInDb!.Quantity.Should().Be(5);
        }

        [Fact]
        public async Task UpdateCartItemQuantityAsync_WithValidItem_ShouldUpdateQuantity()
        {
            // Arrange
            var userId = 1;
            var user = new User { Id = userId, Email = "test@example.com", PasswordHash = "hash", FirstName = "Test", LastName = "User" };
            await _context.Users.AddAsync(user);

            var originalUpdateTime = DateTime.UtcNow.AddHours(-1);
            var cartItem = new ShoppingCartItem
            {
                UserId = userId,
                ProductId = 10,
                Quantity = 2,
                CreatedAt = originalUpdateTime,
                UpdatedAt = originalUpdateTime
            };
            await _context.ShoppingCartItems.AddAsync(cartItem);
            await _context.SaveChangesAsync();

            var newQuantity = 5;

            // Add a small delay to ensure time difference
            await Task.Delay(10);

            // Act
            var result = await _repository.UpdateCartItemQuantityAsync(cartItem.Id, newQuantity);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(cartItem.Id);
            result.Quantity.Should().Be(newQuantity);
            result.UpdatedAt.Should().BeAfter(originalUpdateTime);
        }

        [Fact]
        public async Task UpdateCartItemQuantityAsync_WithInvalidId_ShouldThrowArgumentException()
        {
            // Arrange
            var invalidId = 999;
            var newQuantity = 5;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _repository.UpdateCartItemQuantityAsync(invalidId, newQuantity));

            exception.Message.Should().Contain($"Cart item with ID {invalidId} not found");
        }

        [Fact]
        public async Task RemoveFromCartAsync_WithValidId_ShouldRemoveAndReturnTrue()
        {
            // Arrange
            var userId = 1;
            var user = new User { Id = userId, Email = "test@example.com", PasswordHash = "hash", FirstName = "Test", LastName = "User" };
            await _context.Users.AddAsync(user);

            var cartItem = new ShoppingCartItem { UserId = userId, ProductId = 10, Quantity = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            await _context.ShoppingCartItems.AddAsync(cartItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.RemoveFromCartAsync(cartItem.Id);

            // Assert
            result.Should().BeTrue();
            var itemInDb = await _context.ShoppingCartItems.FirstOrDefaultAsync(x => x.Id == cartItem.Id);
            itemInDb.Should().BeNull();
        }

        [Fact]
        public async Task RemoveFromCartAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            var invalidId = 999;

            // Act
            var result = await _repository.RemoveFromCartAsync(invalidId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RemoveProductFromCartAsync_WithValidUserAndProduct_ShouldRemoveAndReturnTrue()
        {
            // Arrange
            var userId = 1;
            var productId = 10;
            var user = new User { Id = userId, Email = "test@example.com", PasswordHash = "hash", FirstName = "Test", LastName = "User" };
            await _context.Users.AddAsync(user);

            var cartItem = new ShoppingCartItem { UserId = userId, ProductId = productId, Quantity = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            await _context.ShoppingCartItems.AddAsync(cartItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.RemoveProductFromCartAsync(userId, productId);

            // Assert
            result.Should().BeTrue();
            var itemInDb = await _context.ShoppingCartItems.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);
            itemInDb.Should().BeNull();
        }

        [Fact]
        public async Task RemoveProductFromCartAsync_WithNonExistentItem_ShouldReturnFalse()
        {
            // Arrange
            var userId = 1;
            var productId = 999;

            // Act
            var result = await _repository.RemoveProductFromCartAsync(userId, productId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ClearUserCartAsync_WithItems_ShouldRemoveAllUserItems()
        {
            // Arrange
            var userId1 = 1;
            var userId2 = 2;
            var user1 = new User { Id = userId1, Email = "test1@example.com", PasswordHash = "hash", FirstName = "Test1", LastName = "User" };
            var user2 = new User { Id = userId2, Email = "test2@example.com", PasswordHash = "hash", FirstName = "Test2", LastName = "User" };
            await _context.Users.AddRangeAsync(user1, user2);

            var cartItems = new List<ShoppingCartItem>
            {
                new ShoppingCartItem { UserId = userId1, ProductId = 10, Quantity = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new ShoppingCartItem { UserId = userId1, ProductId = 20, Quantity = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new ShoppingCartItem { UserId = userId2, ProductId = 30, Quantity = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };

            await _context.ShoppingCartItems.AddRangeAsync(cartItems);
            await _context.SaveChangesAsync();

            // Act
            await _repository.ClearUserCartAsync(userId1);

            // Assert
            var user1Items = await _context.ShoppingCartItems.Where(x => x.UserId == userId1).ToListAsync();
            var user2Items = await _context.ShoppingCartItems.Where(x => x.UserId == userId2).ToListAsync();

            user1Items.Should().BeEmpty();
            user2Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetCartItemCountAsync_WithItems_ShouldReturnTotalQuantity()
        {
            // Arrange
            var userId = 1;
            var user = new User { Id = userId, Email = "test@example.com", PasswordHash = "hash", FirstName = "Test", LastName = "User" };
            await _context.Users.AddAsync(user);

            var cartItems = new List<ShoppingCartItem>
            {
                new ShoppingCartItem { UserId = userId, ProductId = 10, Quantity = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new ShoppingCartItem { UserId = userId, ProductId = 20, Quantity = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new ShoppingCartItem { UserId = 2, ProductId = 30, Quantity = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };

            await _context.ShoppingCartItems.AddRangeAsync(cartItems);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCartItemCountAsync(userId);

            // Assert
            result.Should().Be(5); // 2 + 3 = 5 (only user 1's items)
        }

        [Fact]
        public async Task GetCartItemCountAsync_WithEmptyCart_ShouldReturnZero()
        {
            // Arrange
            var userId = 1;

            // Act
            var result = await _repository.GetCartItemCountAsync(userId);

            // Assert
            result.Should().Be(0);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}