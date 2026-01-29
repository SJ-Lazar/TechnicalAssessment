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

    public GroupsController(
        GroupPermissionService groupPermissionService,
        UserContext context)
    {
        _groupPermissionService = groupPermissionService;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GroupDto>>> GetGroups() =>
        Ok(await _context.Groups
            .Where(g => !g.Deleted)
            .Select(g => new GroupDto(g.Id, g.Name ?? string.Empty))
            .ToListAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<GroupDetailDto>> GetGroup(int id)
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

    [HttpGet("permissions")]
    public async Task<ActionResult<IEnumerable<PermissionDto>>> GetAllPermissions()
    {
        var permissions = await _groupPermissionService.GetAllPermissionsAsync();
        var permissionDtos = permissions.Select(p => new PermissionDto(p.Id, p.Name ?? string.Empty));
        return Ok(permissionDtos);
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
    }
}
