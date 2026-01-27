namespace SharedLibrary.Base;

public class BaseEntity
{
    public bool Active { get; set; } = true;
    public bool Deleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
