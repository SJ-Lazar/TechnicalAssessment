using System.Collections.Generic;
using SharedLibrary.GroupModels;

namespace SharedLibrary.PermissionsModels;

public class Permission
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public ICollection<Group> Groups { get; set; } = [];
}
