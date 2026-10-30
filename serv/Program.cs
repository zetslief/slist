using Serv.Db;
using Serv.Methods;
using Serv.DataMethods;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SlistDb>(o => o.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAntiforgery();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o =>
{
    o.ExpireTimeSpan = TimeSpan.FromDays(1);
    o.SlidingExpiration = true;
    o.AccessDeniedPath = "/login";
    o.LoginPath = "/login";
});
builder.Services.AddAuthorizationBuilder().SetFallbackPolicy(
    new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddSingleton<ComponentProvider>();

var app = builder.Build();

app.UseCookiePolicy(new()
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
});

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", PageMethods.GetRootPage).WithName("home").DisableAntiforgery();
app.MapGet("/login", PageMethods.GetLoginPage).WithName("login").DisableAntiforgery()
    .AllowAnonymous();

app.MapPost("/register", DataMethods.RegisterAsync).DisableAntiforgery()
    .AllowAnonymous();
app.MapPost("/login", DataMethods.LoginAsync).DisableAntiforgery()
    .AllowAnonymous();
app.MapPost("/lists/new", ListDataMethods.CreateAsync);
app.MapPost("/items/new", ListDataMethods.AppendItemAsync);
app.MapPost("/items/{itemId}/toggle", ListDataMethods.ToggleItemAsync);

app.Run();