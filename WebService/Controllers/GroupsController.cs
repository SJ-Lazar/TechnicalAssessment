using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesLibrary.Groups.Dtos;
using ServicesLibrary.Groups.Requests;
using ServicesLibrary.Groups.Services;
using SharedLibrary.Contexts;
using SharedLibrary.DTOs;

namespace WebService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly GroupPermissionService _groupPermissionService;
    private readonly UserContext _context;
    private readonly ILogger<GroupsController> _logger;

    public GroupsController(
        GroupPermissionService groupPermissionService,
        UserContext context,
        ILogger<GroupsController> logger)
    {
        _groupPermissionService = groupPermissionService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
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

    [HttpGet("{id}")]
    public async Task<ActionResult<GroupDetailDto>> GetGroup(int id)
    {
        try
        {
            var group = await _groupPermissionService.GetGroupWithDetailsAsync(id);

            if (group == null)
                return NotFound();

            var groupDto = new GroupDetailDto(
                group.Id,
                group.Name ?? string.Empty,
                group.Active,
                group.CreatedAt,
                group.UpdatedAt,
                group.Permissions.Select(p => new PermissionDto(p.Id, p.Name ?? string.Empty)).ToList(),
                group.Users.Select(u => new UserDto(
                    u.Id,
                    u.Email ?? string.Empty,
                    u.Active,
                    u.CreatedAt,
                    u.UpdatedAt,
                    new List<GroupDto>()
                )).ToList()
            );

            return Ok(groupDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group {GroupId}", id);
            return StatusCode(500, "An error occurred while retrieving the group");
        }
    }

    [HttpGet("permissions")]
    public async Task<ActionResult<IEnumerable<PermissionDto>>> GetAllPermissions()
    {
        try
        {
            var permissions = await _groupPermissionService.GetAllPermissionsAsync();
            var permissionDtos = permissions.Select(p => new PermissionDto(p.Id, p.Name ?? string.Empty));
            return Ok(permissionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions");
            return StatusCode(500, "An error occurred while retrieving permissions");
        }
    }

    [HttpGet("{id}/available-permissions")]
    public async Task<ActionResult<IEnumerable<PermissionDto>>> GetAvailablePermissions(int id)
    {
        try
        {
            var permissions = await _groupPermissionService.GetAvailablePermissionsForGroupAsync(id);
            var permissionDtos = permissions.Select(p => new PermissionDto(p.Id, p.Name ?? string.Empty));
            return Ok(permissionDtos);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available permissions for group {GroupId}", id);
            return StatusCode(500, "An error occurred while retrieving available permissions");
        }
    }

    [HttpPost("{id}/permissions")]
    public async Task<ActionResult<GroupDetailDto>> AddPermissionToGroup(int id, [FromBody] AddPermissionToGroupRequest request)
    {
        try
        {
            var group = await _groupPermissionService.AddPermissionToGroupAsync(id, request.PermissionId);
            
            // Reload with all details
            group = await _groupPermissionService.GetGroupWithDetailsAsync(id);

            var groupDto = new GroupDetailDto(
                group!.Id,
                group.Name ?? string.Empty,
                group.Active,
                group.CreatedAt,
                group.UpdatedAt,
                group.Permissions.Select(p => new PermissionDto(p.Id, p.Name ?? string.Empty)).ToList(),
                group.Users.Select(u => new UserDto(
                    u.Id,
                    u.Email ?? string.Empty,
                    u.Active,
                    u.CreatedAt,
                    u.UpdatedAt,
                    new List<GroupDto>()
                )).ToList()
            );

            return Ok(groupDto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding permission to group {GroupId}", id);
            return StatusCode(500, "An error occurred while adding permission to group");
        }
    }

    [HttpDelete("{id}/permissions/{permissionId}")]
    public async Task<ActionResult<GroupDetailDto>> RemovePermissionFromGroup(int id, int permissionId)
    {
        try
        {
            var group = await _groupPermissionService.RemovePermissionFromGroupAsync(id, permissionId);
            
            // Reload with all details
            group = await _groupPermissionService.GetGroupWithDetailsAsync(id);

            var groupDto = new GroupDetailDto(
                group!.Id,
                group.Name ?? string.Empty,
                group.Active,
                group.CreatedAt,
                group.UpdatedAt,
                group.Permissions.Select(p => new PermissionDto(p.Id, p.Name ?? string.Empty)).ToList(),
                group.Users.Select(u => new UserDto(
                    u.Id,
                    u.Email ?? string.Empty,
                    u.Active,
                    u.CreatedAt,
                    u.UpdatedAt,
                    new List<GroupDto>()
                )).ToList()
            );

            return Ok(groupDto);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing permission from group {GroupId}", id);
            return StatusCode(500, "An error occurred while removing permission from group");
        }
    }
}
