using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ServicesLibrary.Users;
using SharedLibrary.Contexts;
using SharedLibrary.UserModels;

namespace ServicesLibrary.Tests.Users;

[TestFixture]
public class DeleteUserServiceTests
{
    private UserContext _context = null!;
    private DeleteUserService _service = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<UserContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UserContext(options);
        _service = new DeleteUserService(_context);

        // Seed test data
        SeedTestData();
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private void SeedTestData()
    {
        var users = new[]
        {
            new User
            {
                Id = 1,
                Email = "activeuser@example.com",
                Active = true,
                Deleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new User
            {
                Id = 2,
                Email = "anotheruser@example.com",
                Active = true,
                Deleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new User
            {
                Id = 3,
                Email = "alreadydeleted@example.com",
                Active = false,
                Deleted = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            }
        };

        _context.Users.AddRange(users);
        _context.SaveChanges();
    }

    [Test]
    public async Task ExecuteAsync_WithValidUserId_SoftDeletesUser()
    {
        // Arrange
        var userId = 1;

        // Act
        var result = await _service.ExecuteAsync(userId);

        // Assert
        Assert.That(result.Deleted, Is.True);
        Assert.That(result.Active, Is.False);
    }

    [Test]
    public async Task ExecuteAsync_SetsDeletedFlagToTrue()
    {
        // Arrange
        var userId = 1;

        // Act
        var result = await _service.ExecuteAsync(userId);

        // Assert
        Assert.That(result.Deleted, Is.True);
    }

    [Test]
    public async Task ExecuteAsync_SetsActiveFlagToFalse()
    {
        // Arrange
        var userId = 1;

        // Act
        var result = await _service.ExecuteAsync(userId);

        // Assert
        Assert.That(result.Active, Is.False);
    }

    [Test]
    public async Task ExecuteAsync_SetsUpdatedAtTimestamp()
    {
        // Arrange
        var userId = 1;
        var beforeDelete = DateTime.UtcNow;

        // Act
        await Task.Delay(10);
        var result = await _service.ExecuteAsync(userId);
        var afterDelete = DateTime.UtcNow;

        // Assert
        Assert.That(result.UpdatedAt, Is.Not.Null);
        Assert.That(result.UpdatedAt!.Value, Is.GreaterThanOrEqualTo(beforeDelete));
        Assert.That(result.UpdatedAt!.Value, Is.LessThanOrEqualTo(afterDelete));
    }

    [Test]
    public void ExecuteAsync_WithNonExistentUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 999;

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.ExecuteAsync(userId));
        Assert.That(ex!.Message, Does.Contain("not found"));
    }

    [Test]
    public void ExecuteAsync_WithAlreadyDeletedUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 3;

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.ExecuteAsync(userId));
        Assert.That(ex!.Message, Does.Contain("already been deleted"));
    }

    [Test]
    public async Task ExecuteAsync_DoesNotRemoveUserFromDatabase()
    {
        // Arrange
        var userId = 1;

        // Act
        await _service.ExecuteAsync(userId);

        // Assert
        var userStillInDb = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId);
        Assert.That(userStillInDb, Is.Not.Null);
    }

    [Test]
    public async Task ExecuteAsync_PersistsChangesToDatabase()
    {
        // Arrange
        var userId = 1;

        // Act
        await _service.ExecuteAsync(userId);

        // Assert
        var userInDb = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId);
        Assert.That(userInDb!.Deleted, Is.True);
        Assert.That(userInDb.Active, Is.False);
    }

    [Test]
    public async Task ExecuteAsync_ReturnsDeletedUser()
    {
        // Arrange
        var userId = 1;

        // Act
        var result = await _service.ExecuteAsync(userId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(userId));
        Assert.That(result.Email, Is.EqualTo("activeuser@example.com"));
    }

    [Test]
    public async Task ExecuteAsync_PreservesUserEmail()
    {
        // Arrange
        var userId = 1;
        var originalEmail = "activeuser@example.com";

        // Act
        var result = await _service.ExecuteAsync(userId);

        // Assert
        Assert.That(result.Email, Is.EqualTo(originalEmail));
    }

    [Test]
    public async Task ExecuteAsync_PreservesCreatedAtTimestamp()
    {
        // Arrange
        var userId = 1;
        var user = await _context.Users.FindAsync(userId);
        var originalCreatedAt = user!.CreatedAt;

        // Act
        var result = await _service.ExecuteAsync(userId);

        // Assert
        Assert.That(result.CreatedAt, Is.EqualTo(originalCreatedAt));
    }

    [Test]
    public async Task ExecuteAsync_MultipleDeletes_OnlyFirstSucceeds()
    {
        // Arrange
        var userId = 1;

        // Act
        await _service.ExecuteAsync(userId);

        // Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.ExecuteAsync(userId));
    }

    [Test]
    public async Task ExecuteAsync_DeleteMultipleUsers_EachDeletedIndependently()
    {
        // Arrange
        var userId1 = 1;
        var userId2 = 2;

        // Act
        await _service.ExecuteAsync(userId1);
        await _service.ExecuteAsync(userId2);

        // Assert
        var user1 = await _context.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == userId1);
        var user2 = await _context.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == userId2);
        Assert.That(user1.Deleted, Is.True);
        Assert.That(user2.Deleted, Is.True);
    }
}
