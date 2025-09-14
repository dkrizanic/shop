using Domain.Models;
using Domain.Models.Read;
using Domain.Repositories;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;

namespace Shop.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IConfigurationSection> _jwtSettingsMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _configurationMock = new Mock<IConfiguration>();
        _jwtSettingsMock = new Mock<IConfigurationSection>();

        // Setup JWT configuration
        _configurationMock.Setup(x => x.GetSection("JwtSettings")).Returns(_jwtSettingsMock.Object);
        _jwtSettingsMock.Setup(x => x["SecretKey"]).Returns("ThisIsASecretKeyForJWTTokenGenerationThatShouldBeAtLeast32CharactersLong");
        _jwtSettingsMock.Setup(x => x["Issuer"]).Returns("TestIssuer");
        _jwtSettingsMock.Setup(x => x["Audience"]).Returns("TestAudience");

        _authService = new AuthService(_userRepositoryMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailDoesNotExist_ShouldCreateUserAndReturnAuthResponse()
    {
        // Arrange
        var registrationDto = new UserRegistrationDTO
        {
            Email = "test@example.com",
            Password = "password123",
            FirstName = "John",
            LastName = "Doe"
        };

        var createdUser = new User
        {
            Id = 1,
            Email = registrationDto.Email,
            FirstName = registrationDto.FirstName,
            LastName = registrationDto.LastName,
            PasswordHash = "hashed_password",
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock.Setup(x => x.EmailExistsAsync(registrationDto.Email))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _authService.RegisterAsync(registrationDto);

        // Assert
        result.Should().NotBeNull();
        result.User.Email.Should().Be(registrationDto.Email);
        result.User.FirstName.Should().Be(registrationDto.FirstName);
        result.User.LastName.Should().Be(registrationDto.LastName);
        result.Token.Should().NotBeNullOrEmpty();

        _userRepositoryMock.Verify(x => x.EmailExistsAsync(registrationDto.Email), Times.Once);
        _userRepositoryMock.Verify(x => x.CreateAsync(It.Is<User>(u =>
            u.Email == registrationDto.Email &&
            u.FirstName == registrationDto.FirstName &&
            u.LastName == registrationDto.LastName)), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailExists_ShouldThrowArgumentException()
    {
        // Arrange
        var registrationDto = new UserRegistrationDTO
        {
            Email = "existing@example.com",
            Password = "password123",
            FirstName = "John",
            LastName = "Doe"
        };

        _userRepositoryMock.Setup(x => x.EmailExistsAsync(registrationDto.Email))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _authService.RegisterAsync(registrationDto));

        exception.Message.Should().Be("Email already exists");
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ShouldReturnAuthResponse()
    {
        // Arrange
        var loginDto = new UserLoginDTO
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var user = new User
        {
            Id = 1,
            Email = loginDto.Email,
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = _authService.HashPassword(loginDto.Password),
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result!.User.Email.Should().Be(loginDto.Email);
        result.User.FirstName.Should().Be("John");
        result.User.LastName.Should().Be("Doe");
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var loginDto = new UserLoginDTO
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsInvalid_ShouldReturnNull()
    {
        // Arrange
        var loginDto = new UserLoginDTO
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        var user = new User
        {
            Id = 1,
            Email = loginDto.Email,
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = _authService.HashPassword("correctpassword"),
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GenerateJwtToken_ShouldReturnValidJwtToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var token = _authService.GenerateJwtToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        jsonToken.Claims.Should().Contain(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" && c.Value == "1");
        jsonToken.Claims.Should().Contain(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress" && c.Value == "test@example.com");
        jsonToken.Claims.Should().Contain(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname" && c.Value == "John");
        jsonToken.Claims.Should().Contain(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname" && c.Value == "Doe");
    }

    [Fact]
    public void HashPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        var password = "testpassword123";

        // Act
        var hashedPassword = _authService.HashPassword(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().NotBe(password);
        hashedPassword.Length.Should().BeGreaterThan(password.Length);
    }

    [Fact]
    public void VerifyPassword_WhenPasswordMatches_ShouldReturnTrue()
    {
        // Arrange
        var password = "testpassword123";
        var hashedPassword = _authService.HashPassword(password);

        // Act
        var result = _authService.VerifyPassword(password, hashedPassword);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WhenPasswordDoesNotMatch_ShouldReturnFalse()
    {
        // Arrange
        var correctPassword = "correctpassword";
        var wrongPassword = "wrongpassword";
        var hashedPassword = _authService.HashPassword(correctPassword);

        // Act
        var result = _authService.VerifyPassword(wrongPassword, hashedPassword);

        // Assert
        result.Should().BeFalse();
    }
}