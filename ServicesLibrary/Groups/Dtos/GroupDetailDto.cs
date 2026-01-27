using SharedLibrary.DTOs;

namespace ServicesLibrary.Groups.Dtos;

public record GroupDetailDto(
    int Id,
    string Name,
    bool Active,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<PermissionDto> Permissions,
    List<UserDto> Users
);
