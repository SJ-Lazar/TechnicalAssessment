using Microsoft.EntityFrameworkCore;
using ServicesLibrary.Groups.Dtos;
using SharedLibrary.Contexts;
using SharedLibrary.DTOs;
using SharedLibrary.UserModels;

namespace ServicesLibrary.Users;

public class EditUserService
{
    private readonly UserContext _context;

    public EditUserService(UserContext context) => _context = context;

    /// <summary>
    /// Updates the specified user's email address, group memberships, and active status asynchronously.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to update. Must refer to an existing, non-deleted user.</param>
    /// <param name="email">The new email address to assign to the user, or null to leave the email unchanged. The email must be unique
    /// among all non-deleted users.</param>
    /// <param name="groupIds">A list of group IDs to assign to the user, or null to leave group memberships unchanged.</param>
    /// <param name="active">The new active status to set for the user, or null to leave the status unchanged.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated user DTO.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no user with the specified userId exists or the user has been deleted.</exception>
    public async Task<UserDto> ExecuteAsync(int userId, string? email = null, List<int>? groupIds = null, bool? active = null)
    {
        var user = await _context.Users.Include(u => u.Groups)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.Deleted) ?? throw new InvalidOperationException($"User with ID {userId} not found or has been deleted");

        var emailExists = await _context.Users.AnyAsync(u => u.Email == email && u.Id != userId && !u.Deleted);

        ThrowExceptionUserEmailAlreadyExists(email, emailExists);

        UpdateEmail(email, user);

        UpdateActiveStatus(active, user);

        await UpdateGroups(groupIds, user);

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToDto(user);
    }

    #region Private Functions
    /// <summary>
    /// Throws an exception if the specified email address is already in use.
    /// </summary>
    /// <param name="email">The email address to check for existence. Can be null or whitespace.</param>
    /// <param name="emailExists">true if the email address already exists; otherwise, false.</param>
    /// <exception cref="InvalidOperationException">Thrown if emailExists is true and email is not null or whitespace, indicating the email is already in use.</exception>
    private static void ThrowExceptionUserEmailAlreadyExists(string? email, bool emailExists)
    {
        if (emailExists && !string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException($"Email '{email}' is already in use");
    }
    /// <summary>
    /// Updates the email address of the specified user if a non-empty email is provided.
    /// </summary>
    /// <param name="email">The new email address to assign to the user. If null, empty, or consists only of white-space characters, the
    /// user's email is not updated.</param>
    /// <param name="user">The user whose email address is to be updated. Must not be null if an email is provided.</param>
    private static void UpdateEmail(string? email, User? user)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            user!.Email = email;
        }
    }
    /// <summary>
    /// Updates the active status of the specified user if a value is provided.
    /// </summary>
    /// <param name="active">The new active status to assign to the user, or null to leave the status unchanged.</param>
    /// <param name="user">The user whose active status is to be updated. This parameter must not be null if <paramref name="active"/> has
    /// a value.</param>
    private static void UpdateActiveStatus(bool? active, User? user)
    {
        if (active.HasValue)
            user!.Active = active.Value;
    }
    /// <summary>
    /// Updates the groups associated with the specified user to match the provided list of group IDs.
    /// </summary>
    /// <remarks>Only groups that are not marked as deleted are assigned to the user. Existing group
    /// associations are cleared before new groups are added.</remarks>
    /// <param name="groupIds">A list of group IDs to assign to the user. If null, the user's groups are not modified. If empty, all groups are
    /// removed from the user.</param>
    /// <param name="user">The user whose group associations are to be updated. Must not be null if groupIds is not null.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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

    private static UserDto MapToDto(User user) => new(
        user.Id,
        user.Email ?? string.Empty,
        user.Active,
        user.CreatedAt,
        user.UpdatedAt,
        user.Groups.Select(g => new GroupDto(g.Id, g.Name ?? string.Empty)).ToList()
    );
    #endregion
}
