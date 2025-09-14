using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Text.Json;

namespace Shop.IntegrationTests.ApiTests
{
    public class ProductControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProductControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [Fact]
        public async Task GetProducts_ShouldReturnSuccessAndProducts()
        {
            // Act
            var response = await _client.GetAsync("/api/Product?pageNumber=1&pageSize=5");

            // Assert
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ProductPagedResult>(content, _jsonOptions);

            result.Should().NotBeNull();
            result!.Data.Should().NotBeEmpty();
            result.Data.Should().HaveCount(c => c <= 5);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetProducts_WithCategory_ShouldReturnFilteredProducts()
        {
            // Act
            var response = await _client.GetAsync("/api/Product?category=beauty&pageSize=3");

            // Assert
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ProductPagedResult>(content, _jsonOptions);

            result.Should().NotBeNull();
            result!.Data.Should().NotBeEmpty();
            result.Data.Should().OnlyContain(p => p.Category == "beauty");
        }

        [Fact]
        public async Task SearchProducts_ShouldReturnMatchingProducts()
        {
            // Act
            var response = await _client.GetAsync("/api/Product/search?q=phone&pageSize=3");

            // Assert
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ProductPagedResult>(content, _jsonOptions);

            result.Should().NotBeNull();
            result!.Data.Should().NotBeEmpty();
            // Products should contain "phone" in title or description
            result.Data.Should().OnlyContain(p =>
                p.Title.Contains("phone", StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains("phone", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetProductById_ValidId_ShouldReturnProduct()
        {
            // Act
            var response = await _client.GetAsync("/api/Product/1");

            // Assert
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<ProductDto>(content, _jsonOptions);

            product.Should().NotBeNull();
            product!.Id.Should().Be(1);
            product.Title.Should().NotBeNullOrEmpty();
            product.Price.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetProductById_InvalidId_ShouldReturnNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/Product/99999");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetCategories_ShouldReturnCategories()
        {
            // Act
            var response = await _client.GetAsync("/api/Product/categories");

            // Assert
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var categories = JsonSerializer.Deserialize<List<CategoryDto>>(content, _jsonOptions);

            categories.Should().NotBeNull();
            categories!.Should().NotBeEmpty();
            categories.Should().OnlyContain(c => !string.IsNullOrEmpty(c.Name) && !string.IsNullOrEmpty(c.Slug));
        }

        [Theory]
        [InlineData(0, 10)] // Invalid page number
        [InlineData(1, 0)]  // Invalid page size
        [InlineData(1, 101)] // Page size too large
        public async Task GetProducts_InvalidParameters_ShouldReturnBadRequest(int pageNumber, int pageSize)
        {
            // Act
            var response = await _client.GetAsync($"/api/Product?pageNumber={pageNumber}&pageSize={pageSize}");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
    }

    // DTOs for deserialization
    public class ProductPagedResult
    {
        public List<ProductDto> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Rating { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Thumbnail { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
    }

    public class CategoryDto
    {
        public string Slug { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}