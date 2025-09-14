using Domain.Models;
using Domain.Repositories;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Shop.UnitTests.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IUserRepository _userRepository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _userRepository = new UserRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hashed_password",
            FirstName = "John",
            LastName = "Doe"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _userRepository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hashed_password",
            FirstName = "John",
            LastName = "Doe"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetByEmailAsync("test@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hashed_password",
            FirstName = "John",
            LastName = "Doe"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetByEmailAsync("TEST@EXAMPLE.COM");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _userRepository.GetByEmailAsync("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateUserAndReturnIt()
    {
        // Arrange
        var user = new User
        {
            Email = "newuser@example.com",
            PasswordHash = "hashed_password",
            FirstName = "Jane",
            LastName = "Smith"
        };

        // Act
        var result = await _userRepository.CreateAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Email.Should().Be("newuser@example.com");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        // Verify in database
        var userInDb = await _context.Users.FindAsync(result.Id);
        userInDb.Should().NotBeNull();
        userInDb!.Email.Should().Be("newuser@example.com");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUserAndReturnIt()
    {
        // Arrange
        var user = new User
        {
            Email = "original@example.com",
            PasswordHash = "original_password",
            FirstName = "Original",
            LastName = "User"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var originalUpdatedAt = user.UpdatedAt;
        await Task.Delay(10); // Small delay to ensure different timestamp

        // Act
        user.FirstName = "Updated";
        user.LastName = "Name";
        var result = await _userRepository.UpdateAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Updated");
        result.LastName.Should().Be("Name");
        result.UpdatedAt.Should().BeAfter(originalUpdatedAt);

        // Verify in database
        var userInDb = await _context.Users.FindAsync(user.Id);
        userInDb.Should().NotBeNull();
        userInDb!.FirstName.Should().Be("Updated");
        userInDb.LastName.Should().Be("Name");
    }

    [Fact]
    public async Task EmailExistsAsync_WhenEmailExists_ShouldReturnTrue()
    {
        // Arrange
        var user = new User
        {
            Email = "existing@example.com",
            PasswordHash = "hashed_password",
            FirstName = "John",
            LastName = "Doe"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.EmailExistsAsync("existing@example.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        var user = new User
        {
            Email = "existing@example.com",
            PasswordHash = "hashed_password",
            FirstName = "John",
            LastName = "Doe"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.EmailExistsAsync("EXISTING@EXAMPLE.COM");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_WhenEmailDoesNotExist_ShouldReturnFalse()
    {
        // Act
        var result = await _userRepository.EmailExistsAsync("nonexistent@example.com");

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}