using Microsoft.EntityFrameworkCore;
using SharedLibrary.GroupModels;
using SharedLibrary.PermissionsModels;
using SharedLibrary.UserModels;

namespace SharedLibrary.Contexts;

public class UserContext : DbContext
{
    #region Constructors
    public UserContext()
    {

    }

    public UserContext(DbContextOptions<UserContext> options) : base(options)
    {

    }
    #endregion

    #region DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Permission> Permissions { get; set; } 
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        #region SeedData
        var users = new[]
{
            new User { Id = 1, Email = "admin@example.com" },
            new User { Id = 2, Email = "user1@example.com" },
            new User { Id = 3, Email = "user2@example.com" }
        };

        var groups = new[]
        {
            new Group { Id = 1, Name = "Admin" },
            new Group { Id = 2, Name = "Level 1" },
            new Group { Id = 3, Name = "Level 2" }
        };

        var permissions = new[]
        {
            new Permission { Id = 1, Name = "ManageUsers" },
            new Permission { Id = 2, Name = "ReadReports" },
            new Permission { Id = 3, Name = "WriteReports" }
        };

        modelBuilder.Entity<User>().HasData(users);
        modelBuilder.Entity<Group>().HasData(groups);
        modelBuilder.Entity<Permission>().HasData(permissions);
        #endregion

        #region ModelBuilderConfigurations
        modelBuilder.Entity<User>()
         .HasMany(u => u.Groups)
         .WithMany(g => g.Users)
         .UsingEntity(j => j.HasData(
             new { GroupsId = 1, UsersId = 1 },
             new { GroupsId = 1, UsersId = 2 },
             new { GroupsId = 2, UsersId = 2 },
             new { GroupsId = 3, UsersId = 3 }
         ));

        modelBuilder.Entity<Group>()
            .HasMany(g => g.Permissions)
            .WithMany(p => p.Groups)
            .UsingEntity(j => j.HasData(
                new { GroupsId = 1, PermissionsId = 1 },
                new { GroupsId = 1, PermissionsId = 2 },
                new { GroupsId = 1, PermissionsId = 3 },
                new { GroupsId = 2, PermissionsId = 2 },
                new { GroupsId = 3, PermissionsId = 2 },
                new { GroupsId = 3, PermissionsId = 3 }
            )); 
        #endregion
    }
}
