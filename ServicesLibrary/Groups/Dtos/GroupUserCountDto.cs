namespace ServicesLibrary.Groups.Dtos;

public record GroupUserCountDto(
    int GroupId,
    string GroupName,
    int UserCount
);
