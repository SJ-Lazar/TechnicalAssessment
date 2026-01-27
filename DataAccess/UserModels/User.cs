using SharedLibrary.Base;
using SharedLibrary.GroupModels;
using System.Collections.Generic;

namespace SharedLibrary.UserModels;

public class User : BaseEntity
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public ICollection<Group> Groups { get; set; } = [];
}
