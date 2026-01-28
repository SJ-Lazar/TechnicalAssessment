using Microsoft.EntityFrameworkCore;
using SharedLibrary.Contexts;
using SharedLibrary.UserModels;

namespace ServicesLibrary.Users;

public class DeleteUserService
{
    private readonly UserContext _context;

    public DeleteUserService(UserContext context) => _context = context;

    public async Task<bool> ExecuteAsync(int userId)
    {
        User user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.Deleted) 
            ?? throw new InvalidOperationException($"User with ID {userId} not found or has already been deleted");

        MarkUserAsDeleted(user);
        await _context.SaveChangesAsync();

        return true;
    }


    #region Private Functions
    private static void MarkUserAsDeleted(User user)
    {
        user.Deleted = true;
        user.Active = false;
        user.UpdatedAt = DateTime.UtcNow;
    }
    #endregion
}
