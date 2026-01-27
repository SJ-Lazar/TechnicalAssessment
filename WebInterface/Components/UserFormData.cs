namespace WebInterface.Components;

public class UserFormData
{
    public string Email { get; set; } = string.Empty;
    public List<int> GroupIds { get; set; } = new();
    public bool Active { get; set; } = true;
}
