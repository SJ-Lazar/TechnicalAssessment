using ServicesLibrary.Groups.Dtos;

namespace SharedLibrary.DTOs;

public record UserDto(
    int Id,
    string Email,
    bool Active,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<GroupDto> Groups
);
