namespace ServicesLibrary.Users;

/// <summary>
/// User statistics model
/// </summary>
public class UserStatistics
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int DeletedUsers { get; set; }
    public Dictionary<string, int> UsersPerGroup { get; set; } = new();
}
