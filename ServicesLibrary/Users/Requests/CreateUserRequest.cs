namespace SharedLibrary.DTOs;

public record CreateUserRequest(
    string Email,
    List<int>? GroupIds = null
);
