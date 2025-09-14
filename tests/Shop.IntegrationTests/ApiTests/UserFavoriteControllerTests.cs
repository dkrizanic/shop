using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Text.Json;
using Domain.Validators;

namespace Shop.IntegrationTests.ApiTests
{
    public class UserFavoriteControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        public UserFavoriteControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [Fact]
        public async Task AddToFavorites_ValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            await ClearDatabase();
            var request = new AddFavoriteRequest
            {
                UserId = "integration-test-user",
                ProductId = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/UserFavorite", request);

            // Assert
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AddFavoriteResponse>(content, _jsonOptions);

            result.Should().NotBeNull();
            result!.Message.Should().Be("Added to favorites");
            result.Favorite.Should().NotBeNull();
            result.Favorite.UserId.Should().Be(request.UserId);
            result.Favorite.ProductId.Should().Be(request.ProductId);
            result.Favorite.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task AddToFavorites_DuplicateRequest_ShouldReturnSuccess()
        {
            // Arrange
            await ClearDatabase();
            var request = new AddFavoriteRequest
            {
                UserId = "integration-test-user",
                ProductId = 1
            };

            // Add favorite first time
            var firstResponse = await _client.PostAsJsonAsync("/api/UserFavorite", request);

            // Act - Try to add same favorite again
            var response = await _client.PostAsJsonAsync("/api/UserFavorite", request);

            // Assert - Should return success with existing favorite
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AddFavoriteResponse>(content, _jsonOptions);

            result.Should().NotBeNull();
            result!.Message.Should().Be("Added to favorites");
            result.Favorite.Should().NotBeNull();
            result.Favorite.UserId.Should().Be(request.UserId);
            result.Favorite.ProductId.Should().Be(request.ProductId);
        }

        [Fact]
        public async Task RemoveFromFavorites_ExistingFavorite_ShouldReturnSuccess()
        {
            // Arrange
            await ClearDatabase();
            var userId = "integration-test-user";
            var productId = 1;

            // Add favorite first
            var addRequest = new AddFavoriteRequest { UserId = userId, ProductId = productId };
            await _client.PostAsJsonAsync("/api/UserFavorite", addRequest);

            // Act
            var response = await _client.DeleteAsync($"/api/UserFavorite?userId={userId}&productId={productId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<MessageResponse>(content, _jsonOptions);

            result.Should().NotBeNull();
            result!.Message.Should().Be("Removed from favorites");
        }

        [Fact]
        public async Task RemoveFromFavorites_NonExistingFavorite_ShouldReturnNotFound()
        {
            // Arrange
            await ClearDatabase();
            var userId = "integration-test-user";
            var productId = 999;

            // Act
            var response = await _client.DeleteAsync($"/api/UserFavorite?userId={userId}&productId={productId}");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task IsFavorite_ExistingFavorite_ShouldReturnTrue()
        {
            // Arrange
            await ClearDatabase();
            var userId = "integration-test-user";
            var productId = 1;

            // Add favorite first
            var addRequest = new AddFavoriteRequest { UserId = userId, ProductId = productId };
            await _client.PostAsJsonAsync("/api/UserFavorite", addRequest);

            // Act
            var response = await _client.GetAsync($"/api/UserFavorite/check?userId={userId}&productId={productId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<IsFavoriteResponse>(content, _jsonOptions);

            result.Should().NotBeNull();
            result!.UserId.Should().Be(userId);
            result.ProductId.Should().Be(productId);
            result.IsFavorite.Should().BeTrue();
        }

        [Fact]
        public async Task IsFavorite_NonExistingFavorite_ShouldReturnFalse()
        {
            // Arrange
            await ClearDatabase();
            var userId = "integration-test-user";
            var productId = 999;

            // Act
            var response = await _client.GetAsync($"/api/UserFavorite/check?userId={userId}&productId={productId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<IsFavoriteResponse>(content, _jsonOptions);

            result.Should().NotBeNull();
            result!.UserId.Should().Be(userId);
            result.ProductId.Should().Be(productId);
            result.IsFavorite.Should().BeFalse();
        }

        [Fact]
        public async Task GetUserFavorites_WithFavorites_ShouldReturnFavoriteIds()
        {
            // Arrange
            await ClearDatabase();
            var userId = "integration-test-user";
            var productIds = new[] { 1, 2, 3 };

            // Add multiple favorites
            foreach (var productId in productIds)
            {
                var addRequest = new AddFavoriteRequest { UserId = userId, ProductId = productId };
                await _client.PostAsJsonAsync("/api/UserFavorite", addRequest);
            }

            // Act
            var response = await _client.GetAsync($"/api/UserFavorite/user/{userId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<UserFavoritesResponse>(content, _jsonOptions);

            result.Should().NotBeNull();
            result!.UserId.Should().Be(userId);
            result.FavoriteProductIds.Should().HaveCount(3);
            result.FavoriteProductIds.Should().BeEquivalentTo(productIds);
        }

        [Fact]
        public async Task GetUserFavorites_NoFavorites_ShouldReturnEmptyList()
        {
            // Arrange
            await ClearDatabase();
            var userId = "integration-test-user-empty";

            // Act
            var response = await _client.GetAsync($"/api/UserFavorite/user/{userId}");

            // Assert
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<UserFavoritesResponse>(content, _jsonOptions);

            result.Should().NotBeNull();
            result!.UserId.Should().Be(userId);
            result.FavoriteProductIds.Should().BeEmpty();
        }

        private async Task ClearDatabase()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Clear existing data
            context.UserFavorites.RemoveRange(context.UserFavorites);
            await context.SaveChangesAsync();
        }
    }

    // DTOs for responses

    public class AddFavoriteResponse
    {
        public string Message { get; set; } = string.Empty;
        public UserFavoriteDto Favorite { get; set; } = new();
    }

    public class UserFavoriteDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MessageResponse
    {
        public string Message { get; set; } = string.Empty;
    }

    public class IsFavoriteResponse
    {
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public bool IsFavorite { get; set; }
    }

    public class UserFavoritesResponse
    {
        public string UserId { get; set; } = string.Empty;
        public List<int> FavoriteProductIds { get; set; } = new();
    }
}