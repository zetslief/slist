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
    public Guid Id { get; }
    public required string Login { get; init; }
    public required string PasswordHash { get; init; }
}

public class List
{
    public Guid Id { get; }
    public required string Title { get; init; }

    public required User User { get; set; }
}

public class Item
{
    public Guid Id { get; }
    public required string Name { get; init; }
    public bool Checked { get; private set; }

    public required List List { get; init; }
}
