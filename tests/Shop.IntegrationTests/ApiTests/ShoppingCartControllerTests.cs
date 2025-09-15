using Domain.Models;
using Domain.Models.Read;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Shop.IntegrationTests.ApiTests
{
    public class ShoppingCartControllerTests : IClassFixture<TestApplicationFactory<Program>>
    {
        private readonly TestApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ShoppingCartControllerTests(TestApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<(User user, string token)> CreateUserAndGetTokenAsync()
        {
            // Register a new user to get a real token
            var email = $"testuser_{Guid.NewGuid()}@example.com";
            var registrationDto = new UserRegistrationDTO
            {
                Email = email,
                Password = "password123",
                FirstName = "Test",
                LastName = "User"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registrationDto);
            var registerContent = await registerResponse.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponseDTO>(registerContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Get the user from the database
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await context.Users.FirstAsync(u => u.Email == email);

            return (user, authResponse!.Token);
        }

        private async Task SeedTestDataAsync(int userId)
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Add some cart items
            var cartItems = new List<ShoppingCartItem>
            {
                new ShoppingCartItem
                {
                    UserId = userId,
                    ProductId = 1,
                    Quantity = 2,
                    CreatedAt = DateTime.UtcNow.AddHours(-1),
                    UpdatedAt = DateTime.UtcNow.AddHours(-1)
                },
                new ShoppingCartItem
                {
                    UserId = userId,
                    ProductId = 2,
                    Quantity = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.ShoppingCartItems.AddRange(cartItems);
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetCart_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/shoppingcart");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetCart_WithAuthentication_ShouldReturnCart()
        {
            // Arrange
            var (user, token) = await CreateUserAndGetTokenAsync();
            await SeedTestDataAsync(user.Id);
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/shoppingcart");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var cartJson = await response.Content.ReadAsStringAsync();
            var cart = JsonSerializer.Deserialize<ShoppingCartDTO>(cartJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            cart.Should().NotBeNull();
            cart!.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task AddToCart_WithValidData_ShouldAddItem()
        {
            // Arrange
            var (user, token) = await CreateUserAndGetTokenAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var addToCartDto = new AddToCartDTO
            {
                ProductId = 1,
                Quantity = 3
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/shoppingcart/items", addToCartDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var cartItemJson = await response.Content.ReadAsStringAsync();
            var cartItem = JsonSerializer.Deserialize<ShoppingCartItemDTO>(cartItemJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            cartItem.Should().NotBeNull();
            cartItem!.ProductId.Should().Be(1);
            cartItem.Quantity.Should().Be(3);
        }

        [Fact]
        public async Task AddToCart_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var (user, token) = await CreateUserAndGetTokenAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var invalidDto = new AddToCartDTO
            {
                ProductId = 0, // Invalid product ID
                Quantity = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/shoppingcart/items", invalidDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateCartItem_WithValidData_ShouldUpdateQuantity()
        {
            // Arrange
            var (user, token) = await CreateUserAndGetTokenAsync();
            await SeedTestDataAsync(user.Id);
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var updateDto = new UpdateCartItemDTO
            {
                Quantity = 5
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/shoppingcart/items/1", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var cartItemJson = await response.Content.ReadAsStringAsync();
            var cartItem = JsonSerializer.Deserialize<ShoppingCartItemDTO>(cartItemJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            cartItem.Should().NotBeNull();
            cartItem!.Quantity.Should().Be(5);
        }

        [Fact]
        public async Task UpdateCartItem_WithInvalidProductId_ShouldReturnBadRequest()
        {
            // Arrange
            var (user, token) = await CreateUserAndGetTokenAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var updateDto = new UpdateCartItemDTO
            {
                Quantity = 5
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/shoppingcart/items/0", updateDto); // Invalid product ID

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RemoveFromCart_WithValidProductId_ShouldRemoveItem()
        {
            // Arrange
            var (user, token) = await CreateUserAndGetTokenAsync();
            await SeedTestDataAsync(user.Id);
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync("/api/shoppingcart/items/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("Item removed from cart successfully");
        }

        [Fact]
        public async Task RemoveFromCart_WithInvalidProductId_ShouldReturnBadRequest()
        {
            // Arrange
            var (user, token) = await CreateUserAndGetTokenAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync("/api/shoppingcart/items/0"); // Invalid product ID

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RemoveFromCart_WithNonExistentProduct_ShouldReturnNotFound()
        {
            // Arrange
            var (user, token) = await CreateUserAndGetTokenAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync("/api/shoppingcart/items/999"); // Non-existent product

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ClearCart_WithAuthentication_ShouldClearAllItems()
        {
            // Arrange
            var (user, token) = await CreateUserAndGetTokenAsync();
            await SeedTestDataAsync(user.Id);
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.DeleteAsync("/api/shoppingcart");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("Cart cleared successfully");
        }

        [Fact]
        public async Task GetCartItemCount_WithItems_ShouldReturnCount()
        {
            // Arrange
            var (user, token) = await CreateUserAndGetTokenAsync();
            await SeedTestDataAsync(user.Id);
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/shoppingcart/count");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var countJson = await response.Content.ReadAsStringAsync();
            var countResponse = JsonSerializer.Deserialize<Dictionary<string, int>>(countJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            countResponse.Should().NotBeNull();
            countResponse!["count"].Should().Be(3); // 2 + 1 from seeded data
        }

        [Fact]
        public async Task GetCartItemCount_WithEmptyCart_ShouldReturnZero()
        {
            // Arrange
            var (user, token) = await CreateUserAndGetTokenAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/shoppingcart/count");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var countJson = await response.Content.ReadAsStringAsync();
            var countResponse = JsonSerializer.Deserialize<Dictionary<string, int>>(countJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            countResponse.Should().NotBeNull();
            countResponse!["count"].Should().Be(0);
        }
    }
}