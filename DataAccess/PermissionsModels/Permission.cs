using SharedLibrary.Base;
using SharedLibrary.GroupModels;
using System.Collections.Generic;

namespace SharedLibrary.PermissionsModels;

public class Permission : BaseEntity
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public ICollection<Group> Groups { get; set; } = [];
}
