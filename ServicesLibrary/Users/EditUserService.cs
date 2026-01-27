using Microsoft.EntityFrameworkCore;
using SharedLibrary.Contexts;
using SharedLibrary.UserModels;

namespace ServicesLibrary.Users;

public class EditUserService
{
    private readonly UserContext _context;

    public EditUserService(UserContext context) => _context = context;

    public async Task<User> ExecuteAsync(int userId, string? email = null, List<int>? groupIds = null, bool? active = null)
    {
        var user = await _context.Users
            .Include(u => u.Groups)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.Deleted);

        ThrowExceptionIfUserNotFound(userId, user);

        var emailExists = await _context.Users.AnyAsync(u => u.Email == email && u.Id != userId && !u.Deleted);

        ThrowExceptionUserEmailAlreadyExists(email, emailExists);

        UpdateEmail(email, user);

        UpdateActiveStatus(active, user);

        await UpdateGroups(groupIds, user);

        ThrowExceptionIdUserIsNUll(user);

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return user;
    }

    #region Private Functions
    private static void UpdateEmail(string? email, User? user)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            user!.Email = email;
        }
    }
    private async Task UpdateGroups(List<int>? groupIds, User? user)
    {
        if (groupIds != null)
        {
            user!.Groups.Clear();

            if (groupIds.Any())
            {
                var groups = await _context.Groups
                    .Where(g => groupIds.Contains(g.Id) && !g.Deleted)
                    .ToListAsync();

                foreach (var group in groups)
                {
                    user.Groups.Add(group);
                }
            }
        }
    }
    private static void ThrowExceptionIdUserIsNUll(User? user)
    {
        if (user is null)
        {
            throw new ArgumentNullException($"User is null.");
        }
    }
    private static void UpdateActiveStatus(bool? active, User? user)
    {
        if (active.HasValue)
            user!.Active = active.Value;
    }
    private static void ThrowExceptionUserEmailAlreadyExists(string? email, bool emailExists)
    {
        if (emailExists && !string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException($"Email '{email}' is already in use");
    }
    private static void ThrowExceptionIfUserNotFound(int userId, User? user)
    {
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found or has been deleted");
    } 
    #endregion
}
