using Microsoft.AspNetCore.Mvc;
using ServicesLibrary.Groups.Dtos;
using ServicesLibrary.Groups.Services;
using ServicesLibrary.Users;
using SharedLibrary.DTOs;

namespace WebService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    #region Privates Variables
    private readonly AddUserService _addUserService;
    private readonly GetUserService _getUserService;
    private readonly GetGroupService _getGroupService;
    private readonly EditUserService _editUserService;
    private readonly DeleteUserService _deleteUserService;
    private readonly UserCountService _userCountService;
    #endregion

    #region Constructors
    public UsersController(
    AddUserService addUserService,
    GetUserService getUserService,
    GetGroupService getGroupService,
    EditUserService editUserService,
    DeleteUserService deleteUserService,
    UserCountService userCountService)
    {
        _addUserService = addUserService;
        _getUserService = getUserService;
        _getGroupService = getGroupService;
        _editUserService = editUserService;
        _deleteUserService = deleteUserService;
        _userCountService = userCountService;
    }
    #endregion

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers() => Ok(await _getUserService.ExecuteAsync());
    

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id) => (await _getUserService.ExecuteAsync(id)) is { } user? Ok(user) : NotFound();

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            var userDto = await _addUserService.ExecuteAsync(request.Email, request.GroupIds);

            return CreatedAtAction(nameof(GetUser), new { id = userDto.Id }, userDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var userDto = await _editUserService.ExecuteAsync(
                id,
                request.Email,
                request.GroupIds,
                request.Active
            );

            return Ok(userDto);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(int id) => (await _deleteUserService.ExecuteAsync(id)) ? NoContent() : NotFound();

    [HttpGet("groups")]
    public async Task<ActionResult<IEnumerable<GroupDto>>> GetGroups() => Ok(await _getGroupService.ExecuteAsync());


    [HttpGet("count")]
    public async Task<ActionResult<int>> GetTotalUserCount() => Ok(await _userCountService.GetTotalUserCountAsync());
    

    [HttpGet("count/active")]
    public async Task<ActionResult<int>> GetActiveUserCount() => Ok(await _userCountService.GetActiveUserCountAsync());
    

    [HttpGet("count/per-group")]
    public async Task<ActionResult<Dictionary<string, int>>> GetUserCountPerGroup() => Ok(await _userCountService.GetUserCountPerGroupWithNamesAsync());
 

    [HttpGet("count/group/{groupId}")]
    public async Task<ActionResult<int>> GetUserCountForGroup(int groupId) => Ok(await _userCountService.GetUserCountForGroupAsync(groupId));

    [HttpGet("statistics")]
    public async Task<ActionResult<UserStatisticsDto>> GetUserStatistics()
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
}
