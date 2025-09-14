using Domain.Models;
using Domain.Models.Read;
using Domain.Repositories;
using FluentAssertions;
using Infrastructure.Services;
using Moq;

namespace Shop.UnitTests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IDummyJsonService> _mockDummyJsonService;
        private readonly Mock<IUserFavoriteRepository> _mockFavoriteRepository;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockDummyJsonService = new Mock<IDummyJsonService>();
            _mockFavoriteRepository = new Mock<IUserFavoriteRepository>();
            _productService = new ProductService(_mockDummyJsonService.Object, _mockFavoriteRepository.Object);
        }

        [Fact]
        public async Task GetProductsAsync_WithoutUserId_ShouldReturnProductsWithoutFavorites()
        {
            // Arrange
            var parameters = new ProductQueryParameters { PageNumber = 1, PageSize = 2 };
            var products = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Price = 10.99m },
                new Product { Id = 2, Title = "Product 2", Price = 20.99m }
            };
            var pagedResult = new PagedResult<Product>
            {
                Data = products,
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 2,
                TotalPages = 1,
                HasPreviousPage = false,
                HasNextPage = false
            };

            _mockDummyJsonService.Setup(x => x.GetProductsAsync(parameters))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _productService.GetProductsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.Should().AllSatisfy(p => p.IsFavorite.Should().BeFalse());
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetProductsAsync_WithUserId_ShouldMarkFavoritesCorrectly()
        {
            // Arrange
            var userId = "test-user";
            var parameters = new ProductQueryParameters { PageNumber = 1, PageSize = 2 };
            var products = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Price = 10.99m },
                new Product { Id = 2, Title = "Product 2", Price = 20.99m }
            };
            var pagedResult = new PagedResult<Product>
            {
                Data = products,
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 2,
                TotalPages = 1,
                HasPreviousPage = false,
                HasNextPage = false
            };

            _mockDummyJsonService.Setup(x => x.GetProductsAsync(parameters))
                .ReturnsAsync(pagedResult);
            _mockFavoriteRepository.Setup(x => x.GetUserFavoriteProductIdsAsync(userId))
                .ReturnsAsync(new List<int> { 1 });

            // Act
            var result = await _productService.GetProductsAsync(parameters, userId);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            var productList = result.Data.ToList();
            productList[0].IsFavorite.Should().BeTrue(); // Product 1 is favorite
            productList[1].IsFavorite.Should().BeFalse(); // Product 2 is not favorite
        }

        [Fact]
        public async Task GetProductsAsync_WithSearchTerm_ShouldCallSearchMethod()
        {
            // Arrange
            var parameters = new ProductQueryParameters
            {
                PageNumber = 1,
                PageSize = 2,
                SearchTerm = "laptop"
            };
            var products = new List<Product>
            {
                new Product { Id = 1, Title = "Gaming Laptop", Price = 999.99m }
            };
            var pagedResult = new PagedResult<Product>
            {
                Data = products,
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 2,
                TotalPages = 1,
                HasPreviousPage = false,
                HasNextPage = false
            };

            _mockDummyJsonService.Setup(x => x.SearchProductsAsync("laptop", parameters))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _productService.GetProductsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
            _mockDummyJsonService.Verify(x => x.SearchProductsAsync("laptop", parameters), Times.Once);
        }

        [Fact]
        public async Task GetProductsAsync_WithCategory_ShouldCallCategoryMethod()
        {
            // Arrange
            var parameters = new ProductQueryParameters
            {
                PageNumber = 1,
                PageSize = 2,
                Category = "smartphones"
            };
            var products = new List<Product>
            {
                new Product { Id = 1, Title = "iPhone", Price = 999.99m, Category = "smartphones" }
            };
            var pagedResult = new PagedResult<Product>
            {
                Data = products,
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 2,
                TotalPages = 1,
                HasPreviousPage = false,
                HasNextPage = false
            };

            _mockDummyJsonService.Setup(x => x.GetProductsByCategoryAsync("smartphones", parameters))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _productService.GetProductsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
            _mockDummyJsonService.Verify(x => x.GetProductsByCategoryAsync("smartphones", parameters), Times.Once);
        }

        [Fact]
        public async Task GetProductByIdAsync_WithoutUserId_ShouldReturnProductWithoutFavorite()
        {
            // Arrange
            var productId = 1;
            var product = new Product
            {
                Id = productId,
                Title = "Test Product",
                Price = 10.99m
            };

            _mockDummyJsonService.Setup(x => x.GetProductByIdAsync(productId))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(productId);
            result.IsFavorite.Should().BeFalse();
        }

        [Fact]
        public async Task GetProductByIdAsync_WithUserId_ShouldCheckFavoriteStatus()
        {
            // Arrange
            var productId = 1;
            var userId = "test-user";
            var product = new Product
            {
                Id = productId,
                Title = "Test Product",
                Price = 10.99m
            };

            _mockDummyJsonService.Setup(x => x.GetProductByIdAsync(productId))
                .ReturnsAsync(product);
            _mockFavoriteRepository.Setup(x => x.IsFavoriteAsync(userId, productId))
                .ReturnsAsync(true);

            // Act
            var result = await _productService.GetProductByIdAsync(productId, userId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(productId);
            result.IsFavorite.Should().BeTrue();
        }

        [Fact]
        public async Task GetProductByIdAsync_ProductNotFound_ShouldReturnNull()
        {
            // Arrange
            var productId = 999;
            _mockDummyJsonService.Setup(x => x.GetProductByIdAsync(productId))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCategoriesAsync_ShouldReturnCategories()
        {
            // Arrange
            var categories = new List<CategoryDTO>
            {
                new CategoryDTO { Slug = "beauty", Name = "Beauty" },
                new CategoryDTO { Slug = "electronics", Name = "Electronics" }
            };

            _mockDummyJsonService.Setup(x => x.GetCategoriesAsync())
                .ReturnsAsync(categories);

            // Act
            var result = await _productService.GetCategoriesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(c => c.Slug == "beauty");
            result.Should().Contain(c => c.Slug == "electronics");
        }
    }
}