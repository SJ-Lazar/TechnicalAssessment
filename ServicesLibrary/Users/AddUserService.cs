using Microsoft.EntityFrameworkCore;
using ServicesLibrary.Groups.Dtos;
using SharedLibrary.Contexts;
using SharedLibrary.DTOs;
using SharedLibrary.UserModels;

namespace ServicesLibrary.Users;

public class AddUserService
{
    private readonly UserContext _context;

    public AddUserService(UserContext context) => _context = context;

    /// <summary>
    /// Creates a new user with the specified email address and assigns the user to the provided groups asynchronously.
    /// </summary>
    /// <param name="email">The email address for the new user. Cannot be null or empty.</param>
    /// <param name="groupIds">A list of group IDs to which the new user will be assigned. If null, the user will not be assigned to any
    /// groups.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the newly created user DTO.</returns>
    public async Task<UserDto> ExecuteAsync(string email, List<int>? groupIds = null)
    {
        ValidateEmail(email);

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && !u.Deleted);

        ThrowExceptionIfUserAlreadyExists(email, existingUser);

        var user = CreateNewUser(email);

        await AssignGroupsToUser(groupIds, user);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return MapToDto(user);
    }

    #region Private Functions
    /// <summary>
    /// Validates that the specified email address is not null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="email">The email address to validate. Cannot be null, empty, or white space.</param>
    /// <exception cref="ArgumentException">Thrown if the email address is null, empty, or consists only of white-space characters.</exception>
    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
    }
    /// <summary>
    /// Throws an exception if a user with the specified email address already exists.
    /// </summary>
    /// <param name="email">The email address to check for an existing user. Cannot be null.</param>
    /// <param name="existingUser">The user object representing an existing user with the specified email, or null if no such user exists.</param>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="existingUser"/> is not null, indicating that a user with the specified email already
    /// exists.</exception>
    private static void ThrowExceptionIfUserAlreadyExists(string email, User? existingUser)
    {
        if (existingUser is not null)
            throw new InvalidOperationException($"User with email '{email}' already exists");
    } 
    /// <summary>
    /// Creates a new user instance with the specified email address and default property values.
    /// </summary>
    /// <param name="email">The email address to assign to the new user. Cannot be null.</param>
    /// <returns>A new <see cref="User"/> object with the specified email address. The user is active, not deleted, and has the
    /// current UTC time as the creation date.</returns>
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
    /// <summary>
    /// Assigns the specified groups to the given user asynchronously.
    /// </summary>
    /// <param name="groupIds">A list of group IDs to assign to the user. Only groups with IDs in this list and not marked as deleted will be
    /// assigned. Can be null or empty to assign no groups.</param>
    /// <param name="user">The user to whom the groups will be assigned. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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

    private static UserDto MapToDto(User user) => new(
        user.Id,
        user.Email ?? string.Empty,
        user.Active,
        user.Deleted,
        user.CreatedAt,
        user.UpdatedAt,
        user.Groups.Select(g => new GroupDto(g.Id, g.Name ?? string.Empty)).ToList()
    );
    #endregion
}
