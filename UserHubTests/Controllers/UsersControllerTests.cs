using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServicesLibrary.Groups.Dtos;
using ServicesLibrary.Users;
using SharedLibrary.Contexts;
using SharedLibrary.DTOs;
using SharedLibrary.GroupModels;
using SharedLibrary.UserModels;
using WebService.Controllers;

namespace UserHubTests.Controllers;

[TestFixture]
public class UsersControllerTests
{
    private UserContext _context = null!;
    private UsersController _controller = null!;
    private AddUserService _addUserService = null!;
    private EditUserService _editUserService = null!;
    private DeleteUserService _deleteUserService = null!;
    private UserCountService _userCountService = null!;
    private ILogger<UsersController> _logger = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<UserContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UserContext(options);
        
        // Initialize services
        _addUserService = new AddUserService(_context);
        _editUserService = new EditUserService(_context);
        _deleteUserService = new DeleteUserService(_context);
        _userCountService = new UserCountService(_context);
        
        // Create mock logger
        _logger = new LoggerFactory().CreateLogger<UsersController>();
        
        // Initialize controller
        _controller = new UsersController(
            _addUserService,
            _editUserService,
            _deleteUserService,
            _userCountService,
            _context,
            _logger
        );

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
                Email = "admin@example.com",
                Active = true,
                Deleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new User
            {
                Id = 2,
                Email = "user@example.com",
                Active = true,
                Deleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            }
        };

        _context.Groups.AddRange(groups);
        _context.Users.AddRange(users);
        _context.SaveChanges();

        // Add relationship
        var user1 = _context.Users.Include(u => u.Groups).First(u => u.Id == 1);
        user1.Groups.Add(groups[0]);
        _context.SaveChanges();
    }

    #region GetUsers Tests

    [Test]
    public async Task GetUsers_ReturnsOkWithUserList()
    {
        // Act
        var result = await _controller.GetUsers();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var users = okResult!.Value as IEnumerable<UserDto>;
        Assert.That(users, Is.Not.Null);
        Assert.That(users!.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetUsers_ReturnsOnlyNonDeletedUsers()
    {
        // Arrange - Add a deleted user
        var deletedUser = new User
        {
            Id = 3,
            Email = "deleted@example.com",
            Active = false,
            Deleted = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(deletedUser);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetUsers();

        // Assert
        var okResult = result.Result as OkObjectResult;
        var users = okResult!.Value as IEnumerable<UserDto>;
        Assert.That(users!.Count(), Is.EqualTo(2));
        Assert.That(users.Any(u => u.Email == "deleted@example.com"), Is.False);
    }

    #endregion

    #region GetUser Tests

    [Test]
    public async Task GetUser_WithValidId_ReturnsOkWithUser()
    {
        // Act
        var result = await _controller.GetUser(1);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var user = okResult!.Value as UserDto;
        Assert.That(user, Is.Not.Null);
        Assert.That(user!.Id, Is.EqualTo(1));
        Assert.That(user.Email, Is.EqualTo("admin@example.com"));
    }

    [Test]
    public async Task GetUser_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetUser(999);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task GetUser_IncludesGroups()
    {
        // Act
        var result = await _controller.GetUser(1);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var user = okResult!.Value as UserDto;
        Assert.That(user!.Groups, Is.Not.Empty);
        Assert.That(user.Groups.First().Name, Is.EqualTo("Admin"));
    }

    #endregion

    #region CreateUser Tests

    [Test]
    public async Task CreateUser_WithValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CreateUserRequest("newuser@example.com", new List<int> { 1, 2 });

        // Act
        var result = await _controller.CreateUser(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = result.Result as CreatedAtActionResult;
        var user = createdResult!.Value as UserDto;
        Assert.That(user, Is.Not.Null);
        Assert.That(user!.Email, Is.EqualTo("newuser@example.com"));
        Assert.That(user.Groups, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task CreateUser_WithoutGroups_CreatesUserSuccessfully()
    {
        // Arrange
        var request = new CreateUserRequest("nogroups@example.com");

        // Act
        var result = await _controller.CreateUser(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = result.Result as CreatedAtActionResult;
        var user = createdResult!.Value as UserDto;
        Assert.That(user!.Groups, Is.Empty);
    }

    [Test]
    public async Task CreateUser_WithDuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var request = new CreateUserRequest("admin@example.com");

        // Act
        var result = await _controller.CreateUser(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<ConflictObjectResult>());
    }

    [Test]
    public async Task CreateUser_WithEmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateUserRequest("");

        // Act
        var result = await _controller.CreateUser(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    #endregion

    #region UpdateUser Tests

    [Test]
    public async Task UpdateUser_WithValidData_ReturnsOkWithUpdatedUser()
    {
        // Arrange
        var request = new UpdateUserRequest("updated@example.com", new List<int> { 2 }, true);

        // Act
        var result = await _controller.UpdateUser(1, request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var user = okResult!.Value as UserDto;
        Assert.That(user!.Email, Is.EqualTo("updated@example.com"));
        Assert.That(user.Groups, Has.Count.EqualTo(1));
        Assert.That(user.Groups.First().Id, Is.EqualTo(2));
    }

    [Test]
    public async Task UpdateUser_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateUserRequest("test@example.com");

        // Act
        var result = await _controller.UpdateUser(999, request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task UpdateUser_ChangeActiveStatus_UpdatesSuccessfully()
    {
        // Arrange
        var request = new UpdateUserRequest(Active: false);

        // Act
        var result = await _controller.UpdateUser(1, request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var user = okResult!.Value as UserDto;
        Assert.That(user!.Active, Is.False);
    }

    [Test]
    public async Task UpdateUser_WithDuplicateEmail_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateUserRequest("user@example.com"); // Email of user 2

        // Act
        var result = await _controller.UpdateUser(1, request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    #endregion

    #region DeleteUser Tests

    [Test]
    public async Task DeleteUser_WithValidId_ReturnsNoContent()
    {
        // Act
        var result = await _controller.DeleteUser(1);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task DeleteUser_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.DeleteUser(999);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task DeleteUser_SoftDeletesUser()
    {
        // Act
        await _controller.DeleteUser(1);

        // Assert
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == 1);
        Assert.That(user, Is.Not.Null);
        Assert.That(user!.Deleted, Is.True);
        Assert.That(user.Active, Is.False);
    }

    [Test]
    public async Task DeleteUser_UpdatesTimestamp()
    {
        // Arrange
        var beforeDelete = DateTime.UtcNow;

        // Act
        await Task.Delay(10);
        await _controller.DeleteUser(1);

        // Assert
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == 1);
        Assert.That(user!.UpdatedAt, Is.Not.Null);
        Assert.That(user.UpdatedAt!.Value, Is.GreaterThan(beforeDelete));
    }

    #endregion

    #region GetGroups Tests

    [Test]
    public async Task GetGroups_ReturnsOkWithGroupList()
    {
        // Act
        var result = await _controller.GetGroups();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var groups = okResult!.Value as IEnumerable<GroupDto>;
        Assert.That(groups, Is.Not.Null);
        Assert.That(groups!.Count(), Is.EqualTo(3));
    }

    [Test]
    public async Task GetGroups_ReturnsOnlyNonDeletedGroups()
    {
        // Arrange - Add a deleted group
        var deletedGroup = new Group
        {
            Id = 4,
            Name = "Deleted Group",
            Active = false,
            Deleted = true
        };
        _context.Groups.Add(deletedGroup);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetGroups();

        // Assert
        var okResult = result.Result as OkObjectResult;
        var groups = okResult!.Value as IEnumerable<GroupDto>;
        Assert.That(groups!.Count(), Is.EqualTo(3));
        Assert.That(groups.Any(g => g.Name == "Deleted Group"), Is.False);
    }

    #endregion

    #region Integration Tests

    [Test]
    public async Task CompleteUserLifecycle_CreateEditDelete_WorksCorrectly()
    {
        // Create
        var createRequest = new CreateUserRequest("lifecycle@example.com", new List<int> { 1 });
        var createResult = await _controller.CreateUser(createRequest);
        var createdUser = ((createResult.Result as CreatedAtActionResult)!.Value as UserDto)!;
        var userId = createdUser.Id;

        // Edit
        var updateRequest = new UpdateUserRequest("updated-lifecycle@example.com", new List<int> { 2 }, false);
        var updateResult = await _controller.UpdateUser(userId, updateRequest);
        var updatedUser = ((updateResult.Result as OkObjectResult)!.Value as UserDto)!;

        // Verify Edit
        Assert.That(updatedUser.Email, Is.EqualTo("updated-lifecycle@example.com"));
        Assert.That(updatedUser.Active, Is.False);
        Assert.That(updatedUser.Groups, Has.Count.EqualTo(1));

        // Delete
        var deleteResult = await _controller.DeleteUser(userId);
        Assert.That(deleteResult, Is.InstanceOf<NoContentResult>());

        // Verify Delete
        var getResult = await _controller.GetUser(userId);
        Assert.That(getResult.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task MultipleUsers_WithDifferentGroups_ManagedIndependently()
    {
        // Create user 1 with group 1
        var user1Request = new CreateUserRequest("multi1@example.com", new List<int> { 1 });
        var user1Result = await _controller.CreateUser(user1Request);
        var user1 = ((user1Result.Result as CreatedAtActionResult)!.Value as UserDto)!;

        // Create user 2 with group 2
        var user2Request = new CreateUserRequest("multi2@example.com", new List<int> { 2 });
        var user2Result = await _controller.CreateUser(user2Request);
        var user2 = ((user2Result.Result as CreatedAtActionResult)!.Value as UserDto)!;

        // Verify independence
        Assert.That(user1.Groups.First().Id, Is.EqualTo(1));
        Assert.That(user2.Groups.First().Id, Is.EqualTo(2));

        // Update user 1 doesn't affect user 2
        var updateRequest = new UpdateUserRequest(GroupIds: new List<int> { 3 });
        await _controller.UpdateUser(user1.Id, updateRequest);

        var user2AfterUpdate = await _controller.GetUser(user2.Id);
        var user2Dto = ((user2AfterUpdate.Result as OkObjectResult)!.Value as UserDto)!;
        Assert.That(user2Dto.Groups.First().Id, Is.EqualTo(2));
    }

    #endregion
}
