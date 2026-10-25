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