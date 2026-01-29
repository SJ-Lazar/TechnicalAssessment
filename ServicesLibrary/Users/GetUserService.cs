using Microsoft.EntityFrameworkCore;
using ServicesLibrary.Groups.Dtos;
using SharedLibrary.Contexts;
using SharedLibrary.DTOs;
using SharedLibrary.UserModels;
using System.Linq.Expressions;

namespace ServicesLibrary.Users;

public class GetUserService
{
    private readonly UserContext _context;

    public GetUserService(UserContext context) => _context = context;

    /// <summary>
    /// Retrieves all non-deleted users with their groups.
    /// </summary>
    public async Task<List<UserDto>> ExecuteAsync() => await _context.Users.AsNoTracking().Include(u => u.Groups)
                                                            .Where(u => !u.Deleted).Select(UserProjection).ToListAsync();

    /// <summary>
    /// Retrieves a non-deleted user by id with their groups.
    /// </summary>
    public async Task<UserDto?> ExecuteAsync(int id) => await _context.Users.AsNoTracking().Include(u => u.Groups)
                                                              .Where(u => u.Id == id && !u.Deleted).Select(UserProjection).FirstOrDefaultAsync();

    private static readonly Expression<Func<User, UserDto>> UserProjection = user => new UserDto(
        user.Id,
        user.Email ?? string.Empty,
        user.Active,
        user.Deleted,
        user.CreatedAt,
        user.UpdatedAt,
        user.Groups.Select(g => new GroupDto(g.Id, g.Name ?? string.Empty)).ToList()
    );
}
