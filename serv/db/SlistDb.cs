using Microsoft.EntityFrameworkCore;

namespace Slist.Db;

public sealed class SlistDb : DbContext
{
}

public class List
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
}

public class Item
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public bool Done { get; private set; }
}
