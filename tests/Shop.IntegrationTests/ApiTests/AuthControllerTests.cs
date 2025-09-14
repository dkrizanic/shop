using Domain.Models.Read;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Shop.IntegrationTests.ApiTests;

public class AuthControllerTests : IClassFixture<TestApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly TestApplicationFactory<Program> _factory;

    public AuthControllerTests(TestApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnSuccessAndAuthResponse()
    {
        // Arrange
        var registrationDto = new UserRegistrationDTO
        {
            Email = $"test_{Guid.NewGuid()}@example.com",
            Password = "password123",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registrationDto);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            System.Console.WriteLine($"Registration failed with status {response.StatusCode}: {errorContent}");
        }
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponseDTO>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.User.Should().NotBeNull();
        authResponse.User.Email.Should().Be(registrationDto.Email);
        authResponse.User.FirstName.Should().Be(registrationDto.FirstName);
        authResponse.User.LastName.Should().Be(registrationDto.LastName);
        authResponse.User.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"duplicate_{Guid.NewGuid()}@example.com";
        var registrationDto1 = new UserRegistrationDTO
        {
            Email = email,
            Password = "password123",
            FirstName = "John",
            LastName = "Doe"
        };
        var registrationDto2 = new UserRegistrationDTO
        {
            Email = email,
            Password = "password456",
            FirstName = "Jane",
            LastName = "Smith"
        };

        // Act
        await _client.PostAsJsonAsync("/api/auth/register", registrationDto1);
        var response = await _client.PostAsJsonAsync("/api/auth/register", registrationDto2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Email already exists");
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var registrationDto = new UserRegistrationDTO
        {
            Email = "invalid-email",
            Password = "password123",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registrationDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnSuccessAndAuthResponse()
    {
        // Arrange
        var email = $"login_{Guid.NewGuid()}@example.com";
        var password = "password123";
        var registrationDto = new UserRegistrationDTO
        {
            Email = email,
            Password = password,
            FirstName = "John",
            LastName = "Doe"
        };

        // Register user first
        await _client.PostAsJsonAsync("/api/auth/register", registrationDto);

        var loginDto = new UserLoginDTO
        {
            Email = email,
            Password = password
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponseDTO>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.User.Should().NotBeNull();
        authResponse.User.Email.Should().Be(email);
        authResponse.User.FirstName.Should().Be("John");
        authResponse.User.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginDto = new UserLoginDTO
        {
            Email = "nonexistent@example.com",
            Password = "wrongpassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Invalid email or password");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var email = $"wrongpass_{Guid.NewGuid()}@example.com";
        var registrationDto = new UserRegistrationDTO
        {
            Email = email,
            Password = "correctpassword",
            FirstName = "John",
            LastName = "Doe"
        };

        // Register user first
        await _client.PostAsJsonAsync("/api/auth/register", registrationDto);

        var loginDto = new UserLoginDTO
        {
            Email = email,
            Password = "wrongpassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ShouldReturnUserInfo()
    {
        // Arrange
        var email = $"currentuser_{Guid.NewGuid()}@example.com";
        var registrationDto = new UserRegistrationDTO
        {
            Email = email,
            Password = "password123",
            FirstName = "John",
            LastName = "Doe"
        };

        // Register and get token
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registrationDto);
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponseDTO>(registerContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Set authorization header
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse!.Token);

        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var userDto = JsonSerializer.Deserialize<UserDTO>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        userDto.Should().NotBeNull();
        userDto!.Email.Should().Be(email);
        userDto.FirstName.Should().Be("John");
        userDto.LastName.Should().Be("Doe");
        userDto.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutToken_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid_token");

        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    public async Task Register_WithInvalidEmailFormats_ShouldReturnBadRequest(string invalidEmail)
    {
        // Arrange
        var registrationDto = new UserRegistrationDTO
        {
            Email = invalidEmail,
            Password = "password123",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registrationDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("short")]
    public async Task Register_WithInvalidPasswords_ShouldReturnBadRequest(string invalidPassword)
    {
        // Arrange
        var registrationDto = new UserRegistrationDTO
        {
            Email = $"test_{Guid.NewGuid()}@example.com",
            Password = invalidPassword,
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registrationDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}