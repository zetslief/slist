namespace Serv.Dto;

public record AuthUser(string Login, string Password);

public record NewList(string Title);
public record NewItem(Guid ListId, string Name);
public record List(Guid Id, string Title, DateTime CreateOn);
public record Item(Guid Id, string Name, bool Checked, DateTime CreatedOn);