using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesLibrary.Groups.Dtos;
using ServicesLibrary.Users;
using SharedLibrary.Contexts;
using SharedLibrary.DTOs;

namespace WebService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AddUserService _addUserService;
    private readonly EditUserService _editUserService;
    private readonly DeleteUserService _deleteUserService;
    private readonly UserCountService _userCountService;
    private readonly UserContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        AddUserService addUserService,
        EditUserService editUserService,
        DeleteUserService deleteUserService,
        UserCountService userCountService,
        UserContext context,
        ILogger<UsersController> logger)
    {
        _addUserService = addUserService;
        _editUserService = editUserService;
        _deleteUserService = deleteUserService;
        _userCountService = userCountService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        try
        {
            var users = await _context.Users
                .Include(u => u.Groups)
                .Where(u => !u.Deleted)
                .Select(u => new UserDto(
                    u.Id,
                    u.Email ?? string.Empty,
                    u.Active,
                    u.CreatedAt,
                    u.UpdatedAt,
                    u.Groups.Select(g => new GroupDto(g.Id, g.Name ?? string.Empty)).ToList()
                ))
                .ToListAsync();

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, "An error occurred while retrieving users");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Groups)
                .Where(u => u.Id == id && !u.Deleted)
                .Select(u => new UserDto(
                    u.Id,
                    u.Email ?? string.Empty,
                    u.Active,
                    u.CreatedAt,
                    u.UpdatedAt,
                    u.Groups.Select(g => new GroupDto(g.Id, g.Name ?? string.Empty)).ToList()
                ))
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", id);
            return StatusCode(500, "An error occurred while retrieving the user");
        }
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _addUserService.ExecuteAsync(request.Email, request.GroupIds);

            var userDto = new UserDto(
                user.Id,
                user.Email ?? string.Empty,
                user.Active,
                user.CreatedAt,
                user.UpdatedAt,
                user.Groups.Select(g => new GroupDto(g.Id, g.Name ?? string.Empty)).ToList()
            );

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, "An error occurred while creating the user");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _editUserService.ExecuteAsync(
                id,
                request.Email,
                request.GroupIds,
                request.Active
            );

            var userDto = new UserDto(
                user.Id,
                user.Email ?? string.Empty,
                user.Active,
                user.CreatedAt,
                user.UpdatedAt,
                user.Groups.Select(g => new GroupDto(g.Id, g.Name ?? string.Empty)).ToList()
            );

            return Ok(userDto);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, "An error occurred while updating the user");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        try
        {
            await _deleteUserService.ExecuteAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, "An error occurred while deleting the user");
        }
    }

    [HttpGet("groups")]
    public async Task<ActionResult<IEnumerable<GroupDto>>> GetGroups()
    {
        try
        {
            var groups = await _context.Groups
                .Where(g => !g.Deleted)
                .Select(g => new GroupDto(g.Id, g.Name ?? string.Empty))
                .ToListAsync();

            return Ok(groups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting groups");
            return StatusCode(500, "An error occurred while retrieving groups");
        }
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetTotalUserCount()
    {
        try
        {
            var count = await _userCountService.GetTotalUserCountAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total user count");
            return StatusCode(500, "An error occurred while retrieving user count");
        }
    }

    [HttpGet("count/active")]
    public async Task<ActionResult<int>> GetActiveUserCount()
    {
        try
        {
            var count = await _userCountService.GetActiveUserCountAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active user count");
            return StatusCode(500, "An error occurred while retrieving active user count");
        }
    }

    [HttpGet("count/per-group")]
    public async Task<ActionResult<Dictionary<string, int>>> GetUserCountPerGroup()
    {
        try
        {
            var counts = await _userCountService.GetUserCountPerGroupWithNamesAsync();
            return Ok(counts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user count per group");
            return StatusCode(500, "An error occurred while retrieving user count per group");
        }
    }

    [HttpGet("count/group/{groupId}")]
    public async Task<ActionResult<int>> GetUserCountForGroup(int groupId)
    {
        try
        {
            var count = await _userCountService.GetUserCountForGroupAsync(groupId);
            return Ok(count);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user count for group {GroupId}", groupId);
            return StatusCode(500, "An error occurred while retrieving user count for group");
        }
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<UserStatisticsDto>> GetUserStatistics()
    {
        try
        {
            var stats = await _userCountService.GetUserStatisticsAsync();
            
            var statsDto = new UserStatisticsDto(
                stats.TotalUsers,
                stats.ActiveUsers,
                stats.InactiveUsers,
                stats.DeletedUsers,
                stats.UsersPerGroup
            );

            return Ok(statsDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user statistics");
            return StatusCode(500, "An error occurred while retrieving user statistics");
        }
    }
}
