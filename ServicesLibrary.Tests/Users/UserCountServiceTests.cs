using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ServicesLibrary.Users;
using SharedLibrary.Contexts;
using SharedLibrary.GroupModels;
using SharedLibrary.UserModels;

namespace ServicesLibrary.Tests.Users;

[TestFixture]
public class UserCountServiceTests
{
    private UserContext _context = null!;
    private UserCountService _service = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<UserContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UserContext(options);
        _service = new UserCountService(_context);

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
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 2,
                Email = "user2@example.com",
                Active = true,
                Deleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 3,
                Email = "user3@example.com",
                Active = false,
                Deleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 4,
                Email = "deleted@example.com",
                Active = false,
                Deleted = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Groups.AddRange(groups);
        _context.Users.AddRange(users);
        _context.SaveChanges();

        // Add users to groups
        var user1 = _context.Users.Include(u => u.Groups).First(u => u.Id == 1);
        var user2 = _context.Users.Include(u => u.Groups).First(u => u.Id == 2);
        var user3 = _context.Users.Include(u => u.Groups).First(u => u.Id == 3);

        user1.Groups.Add(groups[0]); // Admin
        user1.Groups.Add(groups[1]); // Users
        
        user2.Groups.Add(groups[1]); // Users
        
        user3.Groups.Add(groups[2]); // Managers

        _context.SaveChanges();
    }

    [Test]
    public async Task GetTotalUserCountAsync_ExcludesDeletedUsers()
    {
        // Act
        var count = await _service.GetTotalUserCountAsync();

        // Assert
        Assert.That(count, Is.EqualTo(3)); // 4 total - 1 deleted = 3
    }

    [Test]
    public async Task GetTotalUserCountIncludingDeletedAsync_IncludesAllUsers()
    {
        // Act
        var count = await _service.GetTotalUserCountIncludingDeletedAsync();

        // Assert
        Assert.That(count, Is.EqualTo(4));
    }

    [Test]
    public async Task GetActiveUserCountAsync_ReturnsOnlyActiveNonDeletedUsers()
    {
        // Act
        var count = await _service.GetActiveUserCountAsync();

        // Assert
        Assert.That(count, Is.EqualTo(2)); // user1 and user2 are active
    }

    [Test]
    public async Task GetUserCountPerGroupAsync_ReturnsCorrectCounts()
    {
        // Act
        var counts = await _service.GetUserCountPerGroupAsync();

        // Assert
        Assert.That(counts, Has.Count.EqualTo(3));
        Assert.That(counts[1], Is.EqualTo(1)); // Admin has 1 user
        Assert.That(counts[2], Is.EqualTo(2)); // Users has 2 users
        Assert.That(counts[3], Is.EqualTo(1)); // Managers has 1 user
    }

    [Test]
    public async Task GetUserCountPerGroupWithNamesAsync_ReturnsCorrectCountsWithNames()
    {
        // Act
        var counts = await _service.GetUserCountPerGroupWithNamesAsync();

        // Assert
        Assert.That(counts, Has.Count.EqualTo(3));
        Assert.That(counts["Admin"], Is.EqualTo(1));
        Assert.That(counts["Users"], Is.EqualTo(2));
        Assert.That(counts["Managers"], Is.EqualTo(1));
    }

    [Test]
    public async Task GetUserCountPerGroupAsync_ExcludesDeletedUsers()
    {
        // Arrange - Add deleted user to a group
        var deletedUser = _context.Users.Include(u => u.Groups).First(u => u.Id == 4);
        var adminGroup = _context.Groups.First(g => g.Id == 1);
        deletedUser.Groups.Add(adminGroup);
        await _context.SaveChangesAsync();

        // Act
        var counts = await _service.GetUserCountPerGroupAsync();

        // Assert - Admin should still have 1 user (deleted user not counted)
        Assert.That(counts[1], Is.EqualTo(1));
    }

    [Test]
    public async Task GetUserCountForGroupAsync_WithValidGroup_ReturnsCorrectCount()
    {
        // Act
        var count = await _service.GetUserCountForGroupAsync(2); // Users group

        // Assert
        Assert.That(count, Is.EqualTo(2));
    }

    [Test]
    public void GetUserCountForGroupAsync_WithNonExistentGroup_ThrowsException()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.GetUserCountForGroupAsync(999));
        Assert.That(ex!.Message, Does.Contain("not found"));
    }

    [Test]
    public async Task GetUserStatisticsAsync_ReturnsComprehensiveStatistics()
    {
        // Act
        var stats = await _service.GetUserStatisticsAsync();

        // Assert
        Assert.That(stats.TotalUsers, Is.EqualTo(3));
        Assert.That(stats.ActiveUsers, Is.EqualTo(2));
        Assert.That(stats.InactiveUsers, Is.EqualTo(1));
        Assert.That(stats.DeletedUsers, Is.EqualTo(1));
        Assert.That(stats.UsersPerGroup, Has.Count.EqualTo(3));
        Assert.That(stats.UsersPerGroup["Admin"], Is.EqualTo(1));
        Assert.That(stats.UsersPerGroup["Users"], Is.EqualTo(2));
        Assert.That(stats.UsersPerGroup["Managers"], Is.EqualTo(1));
    }

    [Test]
    public async Task GetUserCountPerGroupAsync_WithNoUsers_ReturnsZero()
    {
        // Arrange - Remove all users from Admin group
        var user1 = _context.Users.Include(u => u.Groups).First(u => u.Id == 1);
        user1.Groups.Clear();
        await _context.SaveChangesAsync();

        // Act
        var counts = await _service.GetUserCountPerGroupAsync();

        // Assert
        Assert.That(counts[1], Is.EqualTo(0)); // Admin should have 0 users
    }

    [Test]
    public async Task GetTotalUserCountAsync_WithNoUsers_ReturnsZero()
    {
        // Arrange - Delete all users
        _context.Users.RemoveRange(_context.Users);
        await _context.SaveChangesAsync();

        // Act
        var count = await _service.GetTotalUserCountAsync();

        // Assert
        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetUserCountPerGroupAsync_ExcludesDeletedGroups()
    {
        // Arrange - Mark a group as deleted
        var managersGroup = _context.Groups.First(g => g.Id == 3);
        managersGroup.Deleted = true;
        await _context.SaveChangesAsync();

        // Act
        var counts = await _service.GetUserCountPerGroupAsync();

        // Assert
        Assert.That(counts, Has.Count.EqualTo(2)); // Should only have 2 groups
        Assert.That(counts.ContainsKey(3), Is.False); // Managers group excluded
    }

    [Test]
    public async Task GetUserStatisticsAsync_WithMultipleGroupsPerUser_CountsCorrectly()
    {
        // Arrange - user1 already has 2 groups
        var stats = await _service.GetUserStatisticsAsync();

        // Assert - Each user-group relationship counted correctly
        Assert.That(stats.UsersPerGroup["Admin"], Is.EqualTo(1));
        Assert.That(stats.UsersPerGroup["Users"], Is.EqualTo(2));
    }
}
