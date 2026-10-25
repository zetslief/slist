using Serv.Db;
using Serv.Methods;
using Serv.DataMethods;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SlistDb>(o => o.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<ComponentProvider>();
builder.Services.AddAntiforgery();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o =>
{
    o.ExpireTimeSpan = TimeSpan.FromDays(1);
    o.SlidingExpiration = true;
});
builder.Services.AddAuthorization();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

var app = builder.Build();

app.UseCookiePolicy(new()
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
});
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", PageMethods.GetRootPage).WithName("home").DisableAntiforgery();
app.MapGet("/login", PageMethods.GetLoginPage).WithName("login").DisableAntiforgery();

app.MapPost("/register", DataMethods.RegisterAsync).DisableAntiforgery();
app.MapPost("/login", DataMethods.LoginAsync).DisableAntiforgery();

app.Run();