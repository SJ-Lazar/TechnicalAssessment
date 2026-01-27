using System.Collections.Generic;
using SharedLibrary.GroupModels;

namespace SharedLibrary.UserModels;

public class User
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public ICollection<Group> Groups { get; set; } = [];
}
