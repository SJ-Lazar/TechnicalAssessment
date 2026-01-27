namespace SharedLibrary.DTOs;

public record UpdateUserRequest(
    string? Email = null,
    List<int>? GroupIds = null,
    bool? Active = null
);
