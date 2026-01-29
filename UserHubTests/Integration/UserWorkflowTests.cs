using Microsoft.EntityFrameworkCore;
using ServicesLibrary.Users;
using SharedLibrary.Contexts;
using SharedLibrary.DTOs;
using SharedLibrary.GroupModels;
using SharedLibrary.PermissionsModels;
using SharedLibrary.UserModels;

namespace UserHubTests.Integration;

[TestFixture]
public class UserWorkflowTests
{
    private UserContext _context = null!;
    private AddUserService _addUserService = null!;
    private EditUserService _editUserService = null!;
    private DeleteUserService _deleteUserService = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<UserContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UserContext(options);
        _addUserService = new AddUserService(_context);
        _editUserService = new EditUserService(_context);
        _deleteUserService = new DeleteUserService(_context);

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
            new Group { Id = 2, Name = "Level 1", Active = true, Deleted = false },
            new Group { Id = 3, Name = "Level 2", Active = true, Deleted = false }
        };

        var permissions = new[]
        {
            new Permission { Id = 1, Name = "ManageUsers", Active = true, Deleted = false },
            new Permission { Id = 2, Name = "ReadReports", Active = true, Deleted = false },
            new Permission { Id = 3, Name = "WriteReports", Active = true, Deleted = false }
        };

        _context.Groups.AddRange(groups);
        _context.Permissions.AddRange(permissions);
        _context.SaveChanges();

        var adminGroup = _context.Groups.Include(g => g.Permissions).First(g => g.Id == 1);
        adminGroup.Permissions.Add(permissions[0]);
        adminGroup.Permissions.Add(permissions[1]);
        adminGroup.Permissions.Add(permissions[2]);

        var level1Group = _context.Groups.Include(g => g.Permissions).First(g => g.Id == 2);
        level1Group.Permissions.Add(permissions[1]);

        _context.SaveChanges();
    }

    [Test]
    public async Task NewUserJourney_FromCreationToPromotion()
    {
        var newUser = await _addUserService.ExecuteAsync("newemployee@company.com", new List<int> { 2 });
        Assert.That(newUser.Email, Is.EqualTo("newemployee@company.com"));
        Assert.That(newUser.Groups, Has.Count.EqualTo(1));
        Assert.That(newUser.Groups.First().Name, Is.EqualTo("Level 1"));

        var promoted = await _editUserService.ExecuteAsync(
            newUser.Id,
            groupIds: new List<int> { 3 }
        );
        Assert.That(promoted.Groups, Has.Count.EqualTo(1));
        Assert.That(promoted.Groups.First().Name, Is.EqualTo("Level 2"));

        var admin = await _editUserService.ExecuteAsync(
            newUser.Id,
            groupIds: new List<int> { 1, 3 }
        );
        Assert.That(admin.Groups, Has.Count.EqualTo(2));
        Assert.That(admin.Groups.Any(g => g.Name == "Admin"), Is.True);

        var userInDb = await _context.Users
            .Include(u => u.Groups)
            .FirstOrDefaultAsync(u => u.Id == newUser.Id);
        Assert.That(userInDb, Is.Not.Null);
        Assert.That(userInDb!.Groups, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task UserDeactivation_ThenReactivation_WorksCorrectly()
    {
        var user = await _addUserService.ExecuteAsync("employee@company.com", new List<int> { 2 });
        Assert.That(user.Active, Is.True);

        var deactivated = await _editUserService.ExecuteAsync(user.Id, active: false);
        Assert.That(deactivated.Active, Is.False);
        Assert.That(deactivated.Deleted, Is.False); // Not deleted, just inactive

        var reactivated = await _editUserService.ExecuteAsync(user.Id, active: true);
        Assert.That(reactivated.Active, Is.True);
    }

    [Test]
    public async Task EmailChange_WithExistingGroups_PreservesGroups()
    {
        var user = await _addUserService.ExecuteAsync("original@company.com", new List<int> { 1, 2 });
        var originalGroups = user.Groups.Count;

        var updated = await _editUserService.ExecuteAsync(user.Id, email: "updated@company.com");

        Assert.That(updated.Email, Is.EqualTo("updated@company.com"));
        Assert.That(updated.Groups, Has.Count.EqualTo(originalGroups));
    }

    [Test]
    public async Task BulkUserCreation_AllSucceed()
    {
        var emails = new[] { "user1@company.com", "user2@company.com", "user3@company.com" };
        var users = new List<UserDto>();

        foreach (var email in emails)
        {
            var user = await _addUserService.ExecuteAsync(email, new List<int> { 2 });
            users.Add(user);
        }

        Assert.That(users, Has.Count.EqualTo(3));
        Assert.That(users.All(u => u.Active), Is.True);
        Assert.That(users.All(u => !u.Deleted), Is.True);

        var dbUsers = await _context.Users.Where(u => !u.Deleted).ToListAsync();
        Assert.That(dbUsers, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task UserDeletion_PreventsSubsequentUpdates()
    {
        var user = await _addUserService.ExecuteAsync("todelete@company.com");
        await _deleteUserService.ExecuteAsync(user.Id);

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _editUserService.ExecuteAsync(user.Id, email: "newemailshouldfail@company.com")
        );
    }

    [Test]
    public async Task MultipleGroupChanges_TrackCorrectly()
    {
        var user = await _addUserService.ExecuteAsync("groupchanger@company.com");

        await _editUserService.ExecuteAsync(user.Id, groupIds: new List<int> { 2 });
        var afterFirst = await _context.Users.Include(u => u.Groups).FirstAsync(u => u.Id == user.Id);
        Assert.That(afterFirst.Groups, Has.Count.EqualTo(1));

        await _editUserService.ExecuteAsync(user.Id, groupIds: new List<int> { 3 });
        var afterSecond = await _context.Users.Include(u => u.Groups).FirstAsync(u => u.Id == user.Id);
        Assert.That(afterSecond.Groups, Has.Count.EqualTo(1));
        Assert.That(afterSecond.Groups.First().Name, Is.EqualTo("Level 2"));

        await _editUserService.ExecuteAsync(user.Id, groupIds: new List<int> { 1, 2, 3 });
        var afterThird = await _context.Users.Include(u => u.Groups).FirstAsync(u => u.Id == user.Id);
        Assert.That(afterThird.Groups, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task TimestampTracking_ThroughUserLifecycle()
    {
        var user = await _addUserService.ExecuteAsync("timestamps@company.com");
        var createdAt = user.CreatedAt;
        Assert.That(user.UpdatedAt, Is.Null);

        await Task.Delay(10);
        var afterEdit = await _editUserService.ExecuteAsync(user.Id, active: false);
        Assert.That(afterEdit.CreatedAt, Is.EqualTo(createdAt));
        Assert.That(afterEdit.UpdatedAt, Is.Not.Null);
        Assert.That(afterEdit.UpdatedAt!.Value, Is.GreaterThanOrEqualTo(createdAt));
        var firstUpdateTime = afterEdit.UpdatedAt!.Value;

        await Task.Delay(10);
        var afterSecondEdit = await _editUserService.ExecuteAsync(user.Id, email: "newemail@company.com");
        Assert.That(afterSecondEdit.UpdatedAt, Is.Not.Null);
        Assert.That(afterSecondEdit.UpdatedAt!.Value, Is.GreaterThanOrEqualTo(firstUpdateTime));

        await Task.Delay(10);
        var deleted = await _deleteUserService.ExecuteAsync(user.Id);
        Assert.That(deleted.UpdatedAt, Is.Not.Null);
        Assert.That(deleted.UpdatedAt!.Value, Is.GreaterThanOrEqualTo(afterSecondEdit.UpdatedAt!.Value));
        
        Assert.That(deleted.CreatedAt, Is.EqualTo(createdAt));
        Assert.That(deleted.UpdatedAt!.Value, Is.GreaterThanOrEqualTo(createdAt));
    }

    [Test]
    public async Task ConcurrentUserOperations_AreIsolated()
    {
        var user1 = await _addUserService.ExecuteAsync("concurrent1@company.com", new List<int> { 1 });
        var user2 = await _addUserService.ExecuteAsync("concurrent2@company.com", new List<int> { 2 });

        var task1 = _editUserService.ExecuteAsync(user1.Id, email: "updated1@company.com");
        var task2 = _editUserService.ExecuteAsync(user2.Id, email: "updated2@company.com");

        await Task.WhenAll(task1, task2);

        var updated1 = await _context.Users.FindAsync(user1.Id);
        var updated2 = await _context.Users.FindAsync(user2.Id);

        Assert.That(updated1!.Email, Is.EqualTo("updated1@company.com"));
        Assert.That(updated2!.Email, Is.EqualTo("updated2@company.com"));
    }

    [Test]
    public async Task GroupPermissions_IndirectlyValidated()
    {
        var admin = await _addUserService.ExecuteAsync("admin@company.com", new List<int> { 1 });

        var adminInDb = await _context.Users
            .Include(u => u.Groups)
                .ThenInclude(g => g.Permissions)
            .FirstAsync(u => u.Id == admin.Id);

        var adminGroup = adminInDb.Groups.First(g => g.Name == "Admin");
        Assert.That(adminGroup.Permissions, Has.Count.EqualTo(3));
        Assert.That(adminGroup.Permissions.Any(p => p.Name == "ManageUsers"), Is.True);

        var level1User = await _addUserService.ExecuteAsync("level1@company.com", new List<int> { 2 });
        var level1InDb = await _context.Users
            .Include(u => u.Groups)
                .ThenInclude(g => g.Permissions)
            .FirstAsync(u => u.Id == level1User.Id);

        var level1Group = level1InDb.Groups.First(g => g.Name == "Level 1");
        Assert.That(level1Group.Permissions, Has.Count.EqualTo(1));
        Assert.That(level1Group.Permissions.First().Name, Is.EqualTo("ReadReports"));
    }
}
