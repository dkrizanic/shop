using Domain.Models;
using Domain.Models.Read;
using Domain.Repositories;
using FluentAssertions;
using Infrastructure.Services;
using Moq;

namespace Shop.UnitTests.Services
{
    public class ShoppingCartServiceTests
    {
        private readonly Mock<IShoppingCartRepository> _mockCartRepository;
        private readonly Mock<IProductService> _mockProductService;
        private readonly ShoppingCartService _cartService;

        public ShoppingCartServiceTests()
        {
            _mockCartRepository = new Mock<IShoppingCartRepository>();
            _mockProductService = new Mock<IProductService>();
            _cartService = new ShoppingCartService(_mockCartRepository.Object, _mockProductService.Object);
        }

        [Fact]
        public async Task GetUserCartAsync_WithItems_ShouldReturnCartWithProducts()
        {
            // Arrange
            var userId = 1;
            var cartItems = new List<ShoppingCartItem>
            {
                new ShoppingCartItem
                {
                    Id = 1,
                    UserId = userId,
                    ProductId = 10,
                    Quantity = 2,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ShoppingCartItem
                {
                    Id = 2,
                    UserId = userId,
                    ProductId = 20,
                    Quantity = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            var product1 = new ProductDTO { Id = 10, Title = "Product 1", Price = 25.99m };
            var product2 = new ProductDTO { Id = 20, Title = "Product 2", Price = 15.50m };

            _mockCartRepository.Setup(x => x.GetUserCartItemsAsync(userId))
                .ReturnsAsync(cartItems);
            _mockProductService.Setup(x => x.GetProductByIdAsync(10, null))
                .ReturnsAsync(product1);
            _mockProductService.Setup(x => x.GetProductByIdAsync(20, null))
                .ReturnsAsync(product2);

            // Act
            var result = await _cartService.GetUserCartAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalItems.Should().Be(3); // 2 + 1
            result.TotalPrice.Should().Be(67.48m); // (25.99 * 2) + (15.50 * 1)

            var firstItem = result.Items.First(x => x.ProductId == 10);
            firstItem.Quantity.Should().Be(2);
            firstItem.Product.Should().NotBeNull();
            firstItem.Product!.Title.Should().Be("Product 1");
            firstItem.TotalPrice.Should().Be(51.98m);
        }

        [Fact]
        public async Task GetUserCartAsync_WithEmptyCart_ShouldReturnEmptyCart()
        {
            // Arrange
            var userId = 1;
            _mockCartRepository.Setup(x => x.GetUserCartItemsAsync(userId))
                .ReturnsAsync(new List<ShoppingCartItem>());

            // Act
            var result = await _cartService.GetUserCartAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalItems.Should().Be(0);
            result.TotalPrice.Should().Be(0);
            result.LastUpdated.Should().Be(DateTime.MinValue);
        }

        [Fact]
        public async Task GetUserCartAsync_WithNonExistentProduct_ShouldSkipItem()
        {
            // Arrange
            var userId = 1;
            var cartItems = new List<ShoppingCartItem>
            {
                new ShoppingCartItem
                {
                    Id = 1,
                    UserId = userId,
                    ProductId = 999,
                    Quantity = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _mockCartRepository.Setup(x => x.GetUserCartItemsAsync(userId))
                .ReturnsAsync(cartItems);
            _mockProductService.Setup(x => x.GetProductByIdAsync(999, null))
                .ReturnsAsync((ProductDTO?)null);

            // Act
            var result = await _cartService.GetUserCartAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalItems.Should().Be(0);
            result.TotalPrice.Should().Be(0);
        }

        [Fact]
        public async Task AddToCartAsync_WithValidProduct_ShouldAddItem()
        {
            // Arrange
            var userId = 1;
            var addToCartDto = new AddToCartDTO { ProductId = 10, Quantity = 3 };
            var product = new ProductDTO { Id = 10, Title = "Test Product", Price = 19.99m };
            var cartItem = new ShoppingCartItem
            {
                Id = 1,
                UserId = userId,
                ProductId = 10,
                Quantity = 3,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockProductService.Setup(x => x.GetProductByIdAsync(10, null))
                .ReturnsAsync(product);
            _mockCartRepository.Setup(x => x.AddToCartAsync(userId, 10, 3))
                .ReturnsAsync(cartItem);

            // Act
            var result = await _cartService.AddToCartAsync(userId, addToCartDto);

            // Assert
            result.Should().NotBeNull();
            result.ProductId.Should().Be(10);
            result.Quantity.Should().Be(3);
            result.Product.Should().NotBeNull();
            result.Product!.Title.Should().Be("Test Product");
            result.TotalPrice.Should().Be(59.97m); // 19.99 * 3
        }

        [Fact]
        public async Task AddToCartAsync_WithInvalidProduct_ShouldThrowArgumentException()
        {
            // Arrange
            var userId = 1;
            var addToCartDto = new AddToCartDTO { ProductId = 999, Quantity = 1 };

            _mockProductService.Setup(x => x.GetProductByIdAsync(999, null))
                .ReturnsAsync((ProductDTO?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _cartService.AddToCartAsync(userId, addToCartDto));

            exception.Message.Should().Contain("Product with ID 999 not found");
        }

        [Fact]
        public async Task UpdateCartItemQuantityAsync_WithValidItem_ShouldUpdateQuantity()
        {
            // Arrange
            var userId = 1;
            var productId = 10;
            var updateDto = new UpdateCartItemDTO { Quantity = 5 };
            var existingItem = new ShoppingCartItem
            {
                Id = 1,
                UserId = userId,
                ProductId = productId,
                Quantity = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var updatedItem = new ShoppingCartItem
            {
                Id = 1,
                UserId = userId,
                ProductId = productId,
                Quantity = 5,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var product = new ProductDTO { Id = productId, Title = "Test Product", Price = 12.50m };

            _mockCartRepository.Setup(x => x.GetCartItemAsync(userId, productId))
                .ReturnsAsync(existingItem);
            _mockCartRepository.Setup(x => x.UpdateCartItemQuantityAsync(1, 5))
                .ReturnsAsync(updatedItem);
            _mockProductService.Setup(x => x.GetProductByIdAsync(productId, null))
                .ReturnsAsync(product);

            // Act
            var result = await _cartService.UpdateCartItemQuantityAsync(userId, productId, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Quantity.Should().Be(5);
            result.TotalPrice.Should().Be(62.50m); // 12.50 * 5
        }

        [Fact]
        public async Task UpdateCartItemQuantityAsync_WithNonExistentItem_ShouldThrowArgumentException()
        {
            // Arrange
            var userId = 1;
            var productId = 999;
            var updateDto = new UpdateCartItemDTO { Quantity = 5 };

            _mockCartRepository.Setup(x => x.GetCartItemAsync(userId, productId))
                .ReturnsAsync((ShoppingCartItem?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _cartService.UpdateCartItemQuantityAsync(userId, productId, updateDto));

            exception.Message.Should().Contain($"Cart item for product {productId} not found for user {userId}");
        }

        [Fact]
        public async Task RemoveFromCartAsync_WithExistingItem_ShouldReturnTrue()
        {
            // Arrange
            var userId = 1;
            var productId = 10;

            _mockCartRepository.Setup(x => x.RemoveProductFromCartAsync(userId, productId))
                .ReturnsAsync(true);

            // Act
            var result = await _cartService.RemoveFromCartAsync(userId, productId);

            // Assert
            result.Should().BeTrue();
            _mockCartRepository.Verify(x => x.RemoveProductFromCartAsync(userId, productId), Times.Once);
        }

        [Fact]
        public async Task RemoveFromCartAsync_WithNonExistentItem_ShouldReturnFalse()
        {
            // Arrange
            var userId = 1;
            var productId = 999;

            _mockCartRepository.Setup(x => x.RemoveProductFromCartAsync(userId, productId))
                .ReturnsAsync(false);

            // Act
            var result = await _cartService.RemoveFromCartAsync(userId, productId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ClearCartAsync_ShouldCallRepository()
        {
            // Arrange
            var userId = 1;

            // Act
            await _cartService.ClearCartAsync(userId);

            // Assert
            _mockCartRepository.Verify(x => x.ClearUserCartAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetCartItemCountAsync_ShouldReturnCount()
        {
            // Arrange
            var userId = 1;
            var expectedCount = 5;

            _mockCartRepository.Setup(x => x.GetCartItemCountAsync(userId))
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _cartService.GetCartItemCountAsync(userId);

            // Assert
            result.Should().Be(expectedCount);
        }
    }
}