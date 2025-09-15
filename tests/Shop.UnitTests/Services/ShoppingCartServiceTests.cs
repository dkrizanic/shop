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
            var product = new ProductDTO { Id = 10, Title = "Test Product", Price = 19.99m, Stock = 10 };
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
            _mockCartRepository.Setup(x => x.GetCartItemAsync(userId, 10))
                .ReturnsAsync((ShoppingCartItem?)null);
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
        public async Task AddToCartAsync_WithOutOfStockProduct_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var userId = 1;
            var addToCartDto = new AddToCartDTO { ProductId = 10, Quantity = 1 };
            var product = new ProductDTO { Id = 10, Title = "Out of Stock Product", Price = 19.99m, Stock = 0 };

            _mockProductService.Setup(x => x.GetProductByIdAsync(10, null))
                .ReturnsAsync(product);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _cartService.AddToCartAsync(userId, addToCartDto));

            exception.Message.Should().Contain("Out of Stock Product' is currently out of stock");
        }

        [Fact]
        public async Task AddToCartAsync_WithInsufficientStock_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var userId = 1;
            var addToCartDto = new AddToCartDTO { ProductId = 10, Quantity = 5 };
            var product = new ProductDTO { Id = 10, Title = "Low Stock Product", Price = 19.99m, Stock = 3 };

            _mockProductService.Setup(x => x.GetProductByIdAsync(10, null))
                .ReturnsAsync(product);
            _mockCartRepository.Setup(x => x.GetCartItemAsync(userId, 10))
                .ReturnsAsync((ShoppingCartItem?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _cartService.AddToCartAsync(userId, addToCartDto));

            exception.Message.Should().Contain("Only 3 items available for 'Low Stock Product'. Requested: 5");
        }

        [Fact]
        public async Task AddToCartAsync_WithExistingCartItemExceedingStock_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var userId = 1;
            var addToCartDto = new AddToCartDTO { ProductId = 10, Quantity = 2 };
            var product = new ProductDTO { Id = 10, Title = "Limited Stock Product", Price = 19.99m, Stock = 3 };
            var existingCartItem = new ShoppingCartItem
            {
                Id = 1,
                UserId = userId,
                ProductId = 10,
                Quantity = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockProductService.Setup(x => x.GetProductByIdAsync(10, null))
                .ReturnsAsync(product);
            _mockCartRepository.Setup(x => x.GetCartItemAsync(userId, 10))
                .ReturnsAsync(existingCartItem);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _cartService.AddToCartAsync(userId, addToCartDto));

            exception.Message.Should().Contain("Cannot add 2 items. Only 1 more items can be added to cart for 'Limited Stock Product'");
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
            var product = new ProductDTO { Id = productId, Title = "Test Product", Price = 12.50m, Stock = 10 };

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
        public async Task UpdateCartItemQuantityAsync_WithInsufficientStock_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var userId = 1;
            var productId = 10;
            var updateDto = new UpdateCartItemDTO { Quantity = 10 };
            var existingItem = new ShoppingCartItem
            {
                Id = 1,
                UserId = userId,
                ProductId = productId,
                Quantity = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var product = new ProductDTO { Id = productId, Title = "Limited Stock Product", Price = 12.50m, Stock = 5 };

            _mockCartRepository.Setup(x => x.GetCartItemAsync(userId, productId))
                .ReturnsAsync(existingItem);
            _mockProductService.Setup(x => x.GetProductByIdAsync(productId, null))
                .ReturnsAsync(product);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _cartService.UpdateCartItemQuantityAsync(userId, productId, updateDto));

            exception.Message.Should().Contain("Cannot update quantity to 10. Only 5 items available for 'Limited Stock Product'");
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

        [Fact]
        public async Task ValidateCartForCheckoutAsync_WithEmptyCart_ShouldReturnInvalid()
        {
            // Arrange
            var userId = 1;
            _mockCartRepository.Setup(x => x.GetUserCartItemsAsync(userId))
                .ReturnsAsync(new List<ShoppingCartItem>());

            // Act
            var result = await _cartService.ValidateCartForCheckoutAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Message.Should().Be("Cart is empty");
        }

        [Fact]
        public async Task ValidateCartForCheckoutAsync_WithValidStock_ShouldReturnValid()
        {
            // Arrange
            var userId = 1;
            var cartItems = new List<ShoppingCartItem>
            {
                new ShoppingCartItem { Id = 1, UserId = userId, ProductId = 10, Quantity = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };
            var product = new ProductDTO { Id = 10, Title = "Valid Product", Price = 19.99m, Stock = 5 };

            _mockCartRepository.Setup(x => x.GetUserCartItemsAsync(userId))
                .ReturnsAsync(cartItems);
            _mockProductService.Setup(x => x.GetProductByIdAsync(10, null))
                .ReturnsAsync(product);

            // Act
            var result = await _cartService.ValidateCartForCheckoutAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public async Task ValidateCartForCheckoutAsync_WithOutOfStockProduct_ShouldRemoveItemAndReturnInvalid()
        {
            // Arrange
            var userId = 1;
            var cartItems = new List<ShoppingCartItem>
            {
                new ShoppingCartItem { Id = 1, UserId = userId, ProductId = 10, Quantity = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };
            var product = new ProductDTO { Id = 10, Title = "Out of Stock Product", Price = 19.99m, Stock = 0 };

            _mockCartRepository.Setup(x => x.GetUserCartItemsAsync(userId))
                .ReturnsAsync(cartItems);
            _mockProductService.Setup(x => x.GetProductByIdAsync(10, null))
                .ReturnsAsync(product);

            // Act
            var result = await _cartService.ValidateCartForCheckoutAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].ErrorType.Should().Be(CheckoutErrorType.OutOfStock);
            result.Errors[0].ProductTitle.Should().Be("Out of Stock Product");

            // Verify item was removed from cart
            _mockCartRepository.Verify(x => x.RemoveProductFromCartAsync(userId, 10), Times.Once);
        }

        [Fact]
        public async Task ValidateCartForCheckoutAsync_WithInsufficientStock_ShouldAdjustQuantity()
        {
            // Arrange
            var userId = 1;
            var cartItems = new List<ShoppingCartItem>
            {
                new ShoppingCartItem { Id = 1, UserId = userId, ProductId = 10, Quantity = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };
            var product = new ProductDTO { Id = 10, Title = "Limited Stock Product", Price = 19.99m, Stock = 3 };

            _mockCartRepository.Setup(x => x.GetUserCartItemsAsync(userId))
                .ReturnsAsync(cartItems);
            _mockProductService.Setup(x => x.GetProductByIdAsync(10, null))
                .ReturnsAsync(product);

            // Act
            var result = await _cartService.ValidateCartForCheckoutAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].ErrorType.Should().Be(CheckoutErrorType.InsufficientStock);
            result.Errors[0].RequestedQuantity.Should().Be(5);
            result.Errors[0].AvailableStock.Should().Be(3);
            result.Errors[0].AdjustedQuantity.Should().Be(3);

            result.UpdatedCart.Should().NotBeNull();
            result.UpdatedCart!.Items.Should().HaveCount(1);
            result.UpdatedCart.Items[0].Quantity.Should().Be(3);

            // Verify quantity was updated in database
            _mockCartRepository.Verify(x => x.UpdateCartItemQuantityAsync(1, 3), Times.Once);
        }

        [Fact]
        public async Task ValidateCartForCheckoutAsync_WithNonExistentProduct_ShouldRemoveItem()
        {
            // Arrange
            var userId = 1;
            var cartItems = new List<ShoppingCartItem>
            {
                new ShoppingCartItem { Id = 1, UserId = userId, ProductId = 999, Quantity = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };

            _mockCartRepository.Setup(x => x.GetUserCartItemsAsync(userId))
                .ReturnsAsync(cartItems);
            _mockProductService.Setup(x => x.GetProductByIdAsync(999, null))
                .ReturnsAsync((ProductDTO?)null);

            // Act
            var result = await _cartService.ValidateCartForCheckoutAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].ErrorType.Should().Be(CheckoutErrorType.ProductNotFound);
            result.Errors[0].ProductId.Should().Be(999);

            // Verify item was removed from cart
            _mockCartRepository.Verify(x => x.RemoveProductFromCartAsync(userId, 999), Times.Once);
        }

        [Fact]
        public async Task ValidateCartForCheckoutAsync_WithMixedStockIssues_ShouldHandleAllScenarios()
        {
            // Arrange
            var userId = 1;
            var cartItems = new List<ShoppingCartItem>
            {
                new ShoppingCartItem { Id = 1, UserId = userId, ProductId = 10, Quantity = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }, // Valid
                new ShoppingCartItem { Id = 2, UserId = userId, ProductId = 20, Quantity = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }, // Insufficient stock
                new ShoppingCartItem { Id = 3, UserId = userId, ProductId = 30, Quantity = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }, // Out of stock
                new ShoppingCartItem { Id = 4, UserId = userId, ProductId = 999, Quantity = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow } // Not found
            };

            var product1 = new ProductDTO { Id = 10, Title = "Valid Product", Price = 19.99m, Stock = 5 };
            var product2 = new ProductDTO { Id = 20, Title = "Low Stock Product", Price = 29.99m, Stock = 3 };
            var product3 = new ProductDTO { Id = 30, Title = "Out of Stock Product", Price = 39.99m, Stock = 0 };

            _mockCartRepository.Setup(x => x.GetUserCartItemsAsync(userId))
                .ReturnsAsync(cartItems);
            _mockProductService.Setup(x => x.GetProductByIdAsync(10, null))
                .ReturnsAsync(product1);
            _mockProductService.Setup(x => x.GetProductByIdAsync(20, null))
                .ReturnsAsync(product2);
            _mockProductService.Setup(x => x.GetProductByIdAsync(30, null))
                .ReturnsAsync(product3);
            _mockProductService.Setup(x => x.GetProductByIdAsync(999, null))
                .ReturnsAsync((ProductDTO?)null);

            // Act
            var result = await _cartService.ValidateCartForCheckoutAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(3); // Insufficient stock, out of stock, not found

            result.UpdatedCart.Should().NotBeNull();
            result.UpdatedCart!.Items.Should().HaveCount(2); // Valid product + adjusted quantity product

            // Verify database operations
            _mockCartRepository.Verify(x => x.UpdateCartItemQuantityAsync(2, 3), Times.Once); // Adjust quantity
            _mockCartRepository.Verify(x => x.RemoveProductFromCartAsync(userId, 30), Times.Once); // Remove out of stock
            _mockCartRepository.Verify(x => x.RemoveProductFromCartAsync(userId, 999), Times.Once); // Remove not found
        }
    }
}