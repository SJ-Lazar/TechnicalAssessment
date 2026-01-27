using Microsoft.EntityFrameworkCore;
using SharedLibrary.Contexts;
using SharedLibrary.GroupModels;
using SharedLibrary.PermissionsModels;

namespace ServicesLibrary.Groups.Services;

public class GroupPermissionService
{
    private readonly UserContext _context;

    public GroupPermissionService(UserContext context) => _context = context;

    /// <summary>
    /// Gets a group with all its permissions and users
    /// </summary>
    public async Task<Group?> GetGroupWithDetailsAsync(int groupId)
    {
        return await _context.Groups
            .Include(g => g.Permissions)
            .Include(g => g.Users)
            .FirstOrDefaultAsync(g => g.Id == groupId && !g.Deleted);
    }

    /// <summary>
    /// Gets all permissions
    /// </summary>
    public async Task<List<Permission>> GetAllPermissionsAsync()
    {
        return await _context.Permissions
            .Where(p => !p.Deleted)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Adds a permission to a group
    /// </summary>
    public async Task<Group> AddPermissionToGroupAsync(int groupId, int permissionId)
    {
        var group = await _context.Groups
            .Include(g => g.Permissions)
            .FirstOrDefaultAsync(g => g.Id == groupId && !g.Deleted);

        if (group == null)
        {
            throw new InvalidOperationException($"Group with ID {groupId} not found or has been deleted");
        }

        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Id == permissionId && !p.Deleted);

        if (permission == null)
        {
            throw new InvalidOperationException($"Permission with ID {permissionId} not found or has been deleted");
        }

        // Check if permission is already assigned
        if (group.Permissions.Any(p => p.Id == permissionId))
        {
            throw new InvalidOperationException($"Permission '{permission.Name}' is already assigned to group '{group.Name}'");
        }

        group.Permissions.Add(permission);
        group.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return group;
    }

    /// <summary>
    /// Removes a permission from a group
    /// </summary>
    public async Task<Group> RemovePermissionFromGroupAsync(int groupId, int permissionId)
    {
        var group = await _context.Groups
            .Include(g => g.Permissions)
            .FirstOrDefaultAsync(g => g.Id == groupId && !g.Deleted);

        if (group == null)
        {
            throw new InvalidOperationException($"Group with ID {groupId} not found or has been deleted");
        }

        var permission = group.Permissions.FirstOrDefault(p => p.Id == permissionId);

        if (permission == null)
        {
            throw new InvalidOperationException($"Permission with ID {permissionId} is not assigned to this group");
        }

        group.Permissions.Remove(permission);
        group.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return group;
    }

    /// <summary>
    /// Gets all available permissions not assigned to a specific group
    /// </summary>
    public async Task<List<Permission>> GetAvailablePermissionsForGroupAsync(int groupId)
    {
        var group = await _context.Groups
            .Include(g => g.Permissions)
            .FirstOrDefaultAsync(g => g.Id == groupId && !g.Deleted);

        if (group == null)
        {
            throw new InvalidOperationException($"Group with ID {groupId} not found or has been deleted");
        }

        var assignedPermissionIds = group.Permissions.Select(p => p.Id).ToList();

        return await _context.Permissions
            .Where(p => !p.Deleted && !assignedPermissionIds.Contains(p.Id))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}
