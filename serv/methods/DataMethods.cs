using System.Data;
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;

using Microsoft.AspNetCore.Authentication.Cookies;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Serv.Db;

namespace Serv.DataMethods;

public static class DataMethods
{
    public static async Task<IResult> RegisterAsync([FromForm] Dto.AuthUser user, SlistDb db, IPasswordHasher<User> hasher, HttpContext ctx)
    {
        var hash = hasher.HashPassword(new() { Login = "", PasswordHash = "" }, user.Password); // user is not needed here, as not used by default implemetaiton.
        var createdUser = db.Users.Add(new() { Login = user.Login, PasswordHash = hash });
        await db.SaveChangesAsync();
        var identity = new ClaimsIdentity([new("login", user.Login)], CookieAuthenticationDefaults.AuthenticationScheme);
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new(identity));
        return Results.RedirectToRoute("home");
    }

    public static async Task<IResult> LoginAsync([FromForm] Dto.AuthUser user, SlistDb db, IPasswordHasher<User> hasher, HttpContext ctx)
    {
        var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Login == user.Login);
        if (existingUser is null) return Results.BadRequest("Unknown user");
        var verificationResult = hasher.VerifyHashedPassword(existingUser, existingUser.PasswordHash, user.Password);
        switch (verificationResult)
        {
            case PasswordVerificationResult.Success:
                var identity = new ClaimsIdentity([new("login", user.Login)], CookieAuthenticationDefaults.AuthenticationScheme);
                await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new(identity));
                return Results.RedirectToRoute("home");
            case PasswordVerificationResult.SuccessRehashNeeded: throw new NotImplementedException("Rehashing is needed but not supported.");
            case PasswordVerificationResult.Failed: return Results.Forbid();
            default: 
                throw new NotImplementedException($"Password verification result {verificationResult} is not supported.");
        }
    }
}

public static class ListDataMethods
{
    public async static Task<IResult> CreateAsync(Dto.NewList newList, ClaimsPrincipal principal, SlistDb db)
    {
        var maybeLogin = principal.FindFirstValue("login");
        if (string.IsNullOrEmpty(maybeLogin)) return Results.Forbid();
        var maybeUser = await db.Users.FirstOrDefaultAsync(u => u.Login == maybeLogin);
        if (maybeUser is null) return Results.BadRequest();
        var newListEntity = db.Lists.Add(new() { User = maybeUser, Title = newList.Title, CreatedOn = DateTime.UtcNow, LastModified = DateTime.UtcNow });
        await db.SaveChangesAsync();
        return Results.Json<Dto.List>(new(newListEntity.Entity.Id, newListEntity.Entity.Title, newListEntity.Entity.LastModified));
    }

    public async static Task<IResult> AppendItemAsync(Dto.NewItem newItem, SlistDb db)
    {
        var maybeList = await db.Lists.FirstOrDefaultAsync(l => l.Id == newItem.ListId);
        if (maybeList is null) return Results.BadRequest($"List {newItem.ListId} not found");
        var newItemEntity = db.Items.Add(new()
        {
            Name = newItem.Name,
            Checked = false,
            CreatedOn = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            List = maybeList
        });
        await db.SaveChangesAsync();
        return Results.Json<Dto.Item>(new(newItemEntity.Entity.Id, newItemEntity.Entity.Name, newItemEntity.Entity.Checked, newItemEntity.Entity.CreatedOn));
    }
}