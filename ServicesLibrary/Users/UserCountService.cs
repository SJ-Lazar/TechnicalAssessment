using Microsoft.EntityFrameworkCore;
using SharedLibrary.Contexts;

namespace ServicesLibrary.Users;

public class UserCountService
{
    private readonly UserContext _context;

    public UserCountService(UserContext context) => _context = context;

    /// <summary>
    /// Gets the total count of active, non-deleted users
    /// </summary>
    public async Task<int> GetTotalUserCountAsync()
    {
        return await _context.Users
            .Where(u => !u.Deleted)
            .CountAsync();
    }

    /// <summary>
    /// Gets the count of all users including deleted ones
    /// </summary>
    public async Task<int> GetTotalUserCountIncludingDeletedAsync()
    {
        return await _context.Users.CountAsync();
    }

    /// <summary>
    /// Gets the count of active users only
    /// </summary>
    public async Task<int> GetActiveUserCountAsync()
    {
        return await _context.Users
            .Where(u => u.Active && !u.Deleted)
            .CountAsync();
    }

    /// <summary>
    /// Gets the count of users per group
    /// Returns a dictionary with GroupId as key and user count as value
    /// </summary>
    public async Task<Dictionary<int, int>> GetUserCountPerGroupAsync()
    {
        var groupCounts = await _context.Groups
            .Where(g => !g.Deleted)
            .Select(g => new
            {
                GroupId = g.Id,
                UserCount = g.Users.Count(u => !u.Deleted)
            })
            .ToDictionaryAsync(x => x.GroupId, x => x.UserCount);

        return groupCounts;
    }

    /// <summary>
    /// Gets detailed user count per group with group names
    /// Returns a dictionary with Group name as key and user count as value
    /// </summary>
    public async Task<Dictionary<string, int>> GetUserCountPerGroupWithNamesAsync()
    {
        var groupCounts = await _context.Groups
            .Where(g => !g.Deleted)
            .Select(g => new
            {
                GroupName = g.Name ?? "Unknown",
                UserCount = g.Users.Count(u => !u.Deleted)
            })
            .ToDictionaryAsync(x => x.GroupName, x => x.UserCount);

        return groupCounts;
    }

    /// <summary>
    /// Gets the count of users for a specific group
    /// </summary>
    public async Task<int> GetUserCountForGroupAsync(int groupId)
    {
        var group = await _context.Groups
            .Include(g => g.Users)
            .FirstOrDefaultAsync(g => g.Id == groupId && !g.Deleted);

        if (group == null)
        {
            throw new InvalidOperationException($"Group with ID {groupId} not found or has been deleted");
        }

        return group.Users.Count(u => !u.Deleted);
    }

    /// <summary>
    /// Gets comprehensive user statistics
    /// </summary>
    public async Task<UserStatistics> GetUserStatisticsAsync()
    {
        var totalUsers = await GetTotalUserCountAsync();
        var activeUsers = await GetActiveUserCountAsync();
        var totalIncludingDeleted = await GetTotalUserCountIncludingDeletedAsync();
        var usersPerGroup = await GetUserCountPerGroupWithNamesAsync();

        return new UserStatistics
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            InactiveUsers = totalUsers - activeUsers,
            DeletedUsers = totalIncludingDeleted - totalUsers,
            UsersPerGroup = usersPerGroup
        };
    }
}

/// <summary>
/// User statistics model
/// </summary>
public class UserStatistics
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int DeletedUsers { get; set; }
    public Dictionary<string, int> UsersPerGroup { get; set; } = new();
}
