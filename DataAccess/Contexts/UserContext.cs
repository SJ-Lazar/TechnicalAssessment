using Microsoft.EntityFrameworkCore;
using SharedLibrary.GroupModels;
using SharedLibrary.PermissionsModels;
using SharedLibrary.UserModels;

namespace SharedLibrary.Contexts;

public class UserContext : DbContext
{
    public UserContext()
    {
        
    }

    public UserContext(DbContextOptions<UserContext> options) : base(options)
    {

    }

    public DbSet<User> Users { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Permission> Permissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Groups)
            .WithMany(g => g.Users);

        modelBuilder.Entity<Group>()
            .HasMany(g => g.Permissions)
            .WithMany(p => p.Groups);
    }
}
