namespace Serv.Dto;

public record AuthUser(string Login, string Password);

public record NewList(string Title);
public record List(Guid Id, string Title, DateTime CreateOn);