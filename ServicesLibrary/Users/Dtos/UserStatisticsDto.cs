namespace SharedLibrary.DTOs;

public record UserStatisticsDto(
    int TotalUsers,
    int ActiveUsers,
    int InactiveUsers,
    int DeletedUsers,
    Dictionary<string, int> UsersPerGroup
);
