using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ServicesLibrary.Users;
using SharedLibrary.Contexts;
using SharedLibrary.GroupModels;
using SharedLibrary.UserModels;

namespace ServicesLibrary.Tests.Users;

[TestFixture]
public class AddUserServiceTests
{
    private UserContext _context = null!;
    private AddUserService _service = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<UserContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UserContext(options);
        _service = new AddUserService(_context);

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
            new Group { Id = 3, Name = "Deleted Group", Active = false, Deleted = true }
        };

        _context.Groups.AddRange(groups);
        _context.SaveChanges();
    }

    [Test]
    public async Task ExecuteAsync_WithValidEmail_CreatesUser()
    {
        // Arrange
        var email = "newuser@example.com";

        // Act
        var result = await _service.ExecuteAsync(email);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Email, Is.EqualTo(email));
        Assert.That(result.Active, Is.True);
        Assert.That(result.Id, Is.GreaterThan(0));
    }

    [Test]
    public async Task ExecuteAsync_WithGroups_AssignsGroupsToUser()
    {
        // Arrange
        var email = "userWithGroups@example.com";
        var groupIds = new List<int> { 1, 2 };

        // Act
        var result = await _service.ExecuteAsync(email, groupIds);

        // Assert
        Assert.That(result.Groups, Has.Count.EqualTo(2));
        Assert.That(result.Groups.Any(g => g.Id == 1), Is.True);
        Assert.That(result.Groups.Any(g => g.Id == 2), Is.True);
    }

    [Test]
    public void ExecuteAsync_WithNullEmail_ThrowsArgumentException()
    {
        // Arrange
        string? email = null;

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.ExecuteAsync(email!));
        Assert.That(ex!.Message, Does.Contain("Email is required"));
    }

    [Test]
    public void ExecuteAsync_WithEmptyEmail_ThrowsArgumentException()
    {
        // Arrange
        var email = "";

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.ExecuteAsync(email));
        Assert.That(ex!.Message, Does.Contain("Email is required"));
    }

    [Test]
    public void ExecuteAsync_WithWhitespaceEmail_ThrowsArgumentException()
    {
        // Arrange
        var email = "   ";

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.ExecuteAsync(email));
        Assert.That(ex!.Message, Does.Contain("Email is required"));
    }

    [Test]
    public async Task ExecuteAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var email = "duplicate@example.com";
        await _service.ExecuteAsync(email);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.ExecuteAsync(email));
        Assert.That(ex!.Message, Does.Contain("already exists"));
    }

    [Test]
    public async Task ExecuteAsync_WithDeletedGroup_DoesNotAssignDeletedGroup()
    {
        // Arrange
        var email = "user@example.com";
        var groupIds = new List<int> { 1, 3 }; // 3 is deleted

        // Act
        var result = await _service.ExecuteAsync(email, groupIds);

        // Assert
        Assert.That(result.Groups, Has.Count.EqualTo(1));
        Assert.That(result.Groups.Any(g => g.Id == 1), Is.True);
        Assert.That(result.Groups.Any(g => g.Id == 3), Is.False);
    }

    [Test]
    public async Task ExecuteAsync_WithNonExistentGroups_IgnoresNonExistentGroups()
    {
        // Arrange
        var email = "user@example.com";
        var groupIds = new List<int> { 1, 999 }; // 999 doesn't exist

        // Act
        var result = await _service.ExecuteAsync(email, groupIds);

        // Assert
        Assert.That(result.Groups, Has.Count.EqualTo(1));
        Assert.That(result.Groups.First().Id, Is.EqualTo(1));
    }

    [Test]
    public async Task ExecuteAsync_WithEmptyGroupList_CreatesUserWithoutGroups()
    {
        // Arrange
        var email = "nogroups@example.com";
        var groupIds = new List<int>();

        // Act
        var result = await _service.ExecuteAsync(email, groupIds);

        // Assert
        Assert.That(result.Groups, Is.Empty);
    }

    [Test]
    public async Task ExecuteAsync_SetsCreatedAtTimestamp()
    {
        // Arrange
        var email = "timestamptest@example.com";
        var beforeCreate = DateTime.UtcNow;

        // Act
        await Task.Delay(10); // Small delay to ensure timestamp difference
        var result = await _service.ExecuteAsync(email);
        var afterCreate = DateTime.UtcNow;

        // Assert
        Assert.That(result.CreatedAt, Is.GreaterThanOrEqualTo(beforeCreate));
        Assert.That(result.CreatedAt, Is.LessThanOrEqualTo(afterCreate));
    }

    [Test]
    public async Task ExecuteAsync_PersistsUserToDatabase()
    {
        // Arrange
        var email = "persist@example.com";

        // Act
        var result = await _service.ExecuteAsync(email);

        // Assert
        var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Id == result.Id);
        Assert.That(userInDb, Is.Not.Null);
        Assert.That(userInDb!.Email, Is.EqualTo(email));
    }
}
