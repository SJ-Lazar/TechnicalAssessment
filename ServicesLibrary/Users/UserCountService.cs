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
    public async Task<int> GetTotalUserCountAsync() => await _context.Users.AsNoTracking().Where(u => !u.Deleted).CountAsync();

    /// <summary>
    /// Gets the count of all users including deleted ones
    /// </summary>
    public async Task<int> GetTotalUserCountIncludingDeletedAsync() => await _context.Users .AsNoTracking().CountAsync();

    /// <summary>
    /// Gets the count of active users only
    /// </summary>
    public async Task<int> GetActiveUserCountAsync() => await _context.Users.AsNoTracking().Where(u => u.Active && !u.Deleted).CountAsync();

    /// <summary>
    /// Gets the count of users per group
    /// Returns a dictionary with GroupId as key and user count as value
    /// </summary>
    public async Task<Dictionary<int, int>> GetUserCountPerGroupAsync() =>await _context.Groups.AsNoTracking().Where(g => !g.Deleted)
        .Select(g => new{  GroupId = g.Id,   UserCount = g.Users.Count(u => !u.Deleted) }).ToDictionaryAsync(x => x.GroupId, x => x.UserCount);

    /// <summary>
    /// Gets detailed user count per group with group names
    /// Returns a dictionary with Group name as key and user count as value
    /// </summary>
    public async Task<Dictionary<string, int>> GetUserCountPerGroupWithNamesAsync() => await _context.Groups.AsNoTracking().Where(g => !g.Deleted)
            .Select(g => new { GroupName = g.Name ?? "Unknown",UserCount = g.Users.Count(u => !u.Deleted) })
            .ToDictionaryAsync(x => x.GroupName, x => x.UserCount);

    /// <summary>
    /// Gets the count of users for a specific group
    /// </summary>
    public async Task<int> GetUserCountForGroupAsync(int groupId)
    {
        var group = await _context.Groups.AsNoTracking().Include(g => g.Users).FirstOrDefaultAsync(g => g.Id == groupId && !g.Deleted)
            ?? throw new InvalidOperationException($"Group with ID {groupId} not found or has been deleted"); ;

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
