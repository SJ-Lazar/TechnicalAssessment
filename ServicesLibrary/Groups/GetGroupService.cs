using Microsoft.EntityFrameworkCore;
using ServicesLibrary.Groups.Dtos;
using SharedLibrary.Contexts;

namespace ServicesLibrary.Groups.Services;

public class GetGroupService
{
    private readonly UserContext _context;

    public GetGroupService(UserContext context) => _context = context;

    public async Task<List<GroupDto>> ExecuteAsync()
    {
        return await _context.Groups
            .AsNoTracking()
            .Where(g => !g.Deleted)
            .Select(g => new GroupDto(g.Id, g.Name ?? string.Empty))
            .ToListAsync();
    }
}
