using Microsoft.EntityFrameworkCore;

namespace Serv.Db;

public sealed class SlistDb(DbContextOptions<SlistDb> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<List> Lists { get; set; }
    public DbSet<Item> Items { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().Property(e => e.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<List>().Property(e => e.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<Item>().Property(e => e.Id).ValueGeneratedOnAdd();
    }
}

public class User
{
    public Guid Id { get; } = Guid.NewGuid();
    public required string Login { get; init; }
    public required string PasswordHash { get; init; }
}

public class List
{
    public Guid Id { get; } = Guid.NewGuid();
    public required string Title { get; set; }
    public required DateTime CreatedOn { get; init; } 
    public required DateTime LastModified { get; set; }

    public required User User { get; set; }
}

public class Item
{
    public Guid Id { get; } = Guid.NewGuid();
    public required string Name { get; set; }
    public bool Checked { get; set; }
    public required DateTime CreatedOn { get; init; } 
    public required DateTime LastModified { get; set; } 

    public required List List { get; init; }
}
