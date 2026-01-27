using SharedLibrary.Base;
using SharedLibrary.PermissionsModels;
using SharedLibrary.UserModels;

namespace SharedLibrary.GroupModels;

public class Group : BaseEntity
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public ICollection<User> Users { get; set; } = [];
    public ICollection<Permission> Permissions { get; set; } = [];
}
