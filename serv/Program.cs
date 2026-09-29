using Serv.Db;
using Serv.Methods;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SlistDb>();
builder.Services.AddSingleton<ComponentProvider>();

var app = builder.Build();

app.MapGet("/", PageMethods.GetRootPage);
app.Run();
