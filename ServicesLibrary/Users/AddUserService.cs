using Microsoft.EntityFrameworkCore;
using SharedLibrary.Contexts;
using SharedLibrary.UserModels;

namespace ServicesLibrary.Users;

public class AddUserService
{
    private readonly UserContext _context;

    public AddUserService(UserContext context) => _context = context;

    public async Task<User> ExecuteAsync(string email, List<int>? groupIds = null)
    {
        ValidateEmail(email);

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && !u.Deleted);

        ThrowExceptionIfUserAlreadyExists(email, existingUser);

        var user = CreateNewUser(email);

        await AssignGroupsToUser(groupIds, user);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    #region Private Functions
    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required", nameof(email));
        }
    }
    private static User CreateNewUser(string email)
    {
        return new User
        {
            Email = email,
            Active = true,
            Deleted = false,
            CreatedAt = DateTime.UtcNow
        };
    }
    private async Task AssignGroupsToUser(List<int>? groupIds, User user)
    {
        if (groupIds != null && groupIds.Any())
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
    private static void ThrowExceptionIfUserAlreadyExists(string email, User? existingUser)
    {
        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with email '{email}' already exists");
        }
    } 
    #endregion
}
