using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ServicesLibrary.Users;
using SharedLibrary.Contexts;
using SharedLibrary.GroupModels;
using SharedLibrary.UserModels;

namespace ServicesLibrary.Tests.Users;

[TestFixture]
public class EditUserServiceTests
{
    private UserContext _context = null!;
    private EditUserService _service = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<UserContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UserContext(options);
        _service = new EditUserService(_context);

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
        var groups = new[]
        {
            new Group { Id = 1, Name = "Admin", Active = true, Deleted = false },
            new Group { Id = 2, Name = "Users", Active = true, Deleted = false },
            new Group { Id = 3, Name = "Managers", Active = true, Deleted = false }
        };

        var users = new[]
        {
            new User
            {
                Id = 1,
                Email = "user1@example.com",
                Active = true,
                Deleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new User
            {
                Id = 2,
                Email = "user2@example.com",
                Active = true,
                Deleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new User
            {
                Id = 3,
                Email = "deleted@example.com",
                Active = false,
                Deleted = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            }
        };

        _context.Groups.AddRange(groups);
        _context.Users.AddRange(users);
        _context.SaveChanges();

        // Add group to user 1
        var user1 = _context.Users.Include(u => u.Groups).First(u => u.Id == 1);
        user1.Groups.Add(groups[0]);
        _context.SaveChanges();
    }

    [Test]
    public async Task ExecuteAsync_UpdateEmail_UpdatesUserEmail()
    {
        // Arrange
        var userId = 1;
        var newEmail = "newemail@example.com";

        // Act
        var result = await _service.ExecuteAsync(userId, email: newEmail);

        // Assert
        Assert.That(result.Email, Is.EqualTo(newEmail));
    }

    [Test]
    public async Task ExecuteAsync_UpdateActiveStatus_UpdatesActiveFlag()
    {
        // Arrange
        var userId = 1;

        // Act
        var result = await _service.ExecuteAsync(userId, active: false);

        // Assert
        Assert.That(result.Active, Is.False);
    }

    [Test]
    public async Task ExecuteAsync_UpdateGroups_ReplacesExistingGroups()
    {
        // Arrange
        var userId = 1;
        var newGroupIds = new List<int> { 2, 3 };

        // Act
        var result = await _service.ExecuteAsync(userId, groupIds: newGroupIds);

        // Assert
        Assert.That(result.Groups, Has.Count.EqualTo(2));
        Assert.That(result.Groups.Any(g => g.Id == 2), Is.True);
        Assert.That(result.Groups.Any(g => g.Id == 3), Is.True);
        Assert.That(result.Groups.Any(g => g.Id == 1), Is.False);
    }

    [Test]
    public async Task ExecuteAsync_ClearGroups_RemovesAllGroups()
    {
        // Arrange
        var userId = 1;
        var emptyGroupIds = new List<int>();

        // Act
        var result = await _service.ExecuteAsync(userId, groupIds: emptyGroupIds);

        // Assert
        Assert.That(result.Groups, Is.Empty);
    }

    [Test]
    public async Task ExecuteAsync_NoGroupsParameter_KeepsExistingGroups()
    {
        // Arrange
        var userId = 1;

        // Act
        var result = await _service.ExecuteAsync(userId, email: "updated@example.com");

        // Assert
        Assert.That(result.Groups, Has.Count.EqualTo(1));
        Assert.That(result.Groups.First().Id, Is.EqualTo(1));
    }

    [Test]
    public void ExecuteAsync_WithNonExistentUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 999;

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.ExecuteAsync(userId, email: "test@example.com"));
        Assert.That(ex!.Message, Does.Contain("not found"));
    }

    [Test]
    public void ExecuteAsync_WithDeletedUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 3;

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.ExecuteAsync(userId, email: "test@example.com"));
        Assert.That(ex!.Message, Does.Contain("not found or has been deleted"));
    }

    [Test]
    public void ExecuteAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 1;
        var existingEmail = "user2@example.com"; // Already used by user 2

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.ExecuteAsync(userId, email: existingEmail));
        Assert.That(ex!.Message, Does.Contain("already in use"));
    }

    [Test]
    public async Task ExecuteAsync_WithSameEmail_DoesNotThrowException()
    {
        // Arrange
        var userId = 1;
        var sameEmail = "user1@example.com";

        // Act & Assert
        Assert.DoesNotThrowAsync(
            async () => await _service.ExecuteAsync(userId, email: sameEmail));
    }

    [Test]
    public async Task ExecuteAsync_SetsUpdatedAtTimestamp()
    {
        // Arrange
        var userId = 1;
        var beforeUpdate = DateTime.UtcNow;

        // Act
        await Task.Delay(10);
        var result = await _service.ExecuteAsync(userId, email: "updated@example.com");
        var afterUpdate = DateTime.UtcNow;

        // Assert
        Assert.That(result.UpdatedAt, Is.Not.Null);
        Assert.That(result.UpdatedAt!.Value, Is.GreaterThanOrEqualTo(beforeUpdate));
        Assert.That(result.UpdatedAt!.Value, Is.LessThanOrEqualTo(afterUpdate));
    }

    [Test]
    public async Task ExecuteAsync_UpdatesMultipleFields_AllFieldsUpdated()
    {
        // Arrange
        var userId = 1;
        var newEmail = "multifieldupdate@example.com";
        var newGroupIds = new List<int> { 2, 3 };
        var newActiveStatus = false;

        // Act
        var result = await _service.ExecuteAsync(userId, newEmail, newGroupIds, newActiveStatus);

        // Assert
        Assert.That(result.Email, Is.EqualTo(newEmail));
        Assert.That(result.Active, Is.EqualTo(newActiveStatus));
        Assert.That(result.Groups, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task ExecuteAsync_PersistsChangesToDatabase()
    {
        // Arrange
        var userId = 1;
        var newEmail = "persistent@example.com";

        // Act
        await _service.ExecuteAsync(userId, email: newEmail);

        // Assert
        var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        Assert.That(userInDb!.Email, Is.EqualTo(newEmail));
    }

    [Test]
    public async Task ExecuteAsync_WithNullEmail_DoesNotUpdateEmail()
    {
        // Arrange
        var userId = 1;
        var originalEmail = "user1@example.com";

        // Act
        var result = await _service.ExecuteAsync(userId, email: null, active: false);

        // Assert
        Assert.That(result.Email, Is.EqualTo(originalEmail));
    }

    [Test]
    public async Task ExecuteAsync_WithNullActive_DoesNotUpdateActiveStatus()
    {
        // Arrange
        var userId = 1;
        var originalActive = true;

        // Act
        var result = await _service.ExecuteAsync(userId, email: "test@example.com", active: null);

        // Assert
        Assert.That(result.Active, Is.EqualTo(originalActive));
    }
}
