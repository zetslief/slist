using Slist.Db;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<SlistDb>();

var app = builder.Build();

app.MapGet("/", () => Results.Content(File.ReadAllText("./pages/index.html"), "text/html"));

app.Run();
