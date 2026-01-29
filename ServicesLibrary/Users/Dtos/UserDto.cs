using ServicesLibrary.Groups.Dtos;

namespace SharedLibrary.DTOs;

public record UserDto(
    int Id,
    string Email,
    bool Active,
    bool Deleted,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<GroupDto> Groups
);
