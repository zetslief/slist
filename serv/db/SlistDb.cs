using Microsoft.EntityFrameworkCore;

namespace Serv.Db;

public sealed class SlistDb : DbContext
{
    public DbSet<List> Lists { get; set; }
    public DbSet<Item> Items { get; set; }
}

public class List
{
    public Guid Id { get; private set; }
    public required string Name { get; init; }
}

public class Item
{
    public Guid Id { get; } = Guid.NewGuid();
    public required string Name { get; init; }
    public bool Done { get; private set; }

    public required List List { get; init; }
}
