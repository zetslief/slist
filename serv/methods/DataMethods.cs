using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Serv.Db;

namespace Serv.DataMethods;

public static class DataMethods
{
    public static async Task<IResult> RegisterAsync(Dto.AuthUser user, SlistDb db, IPasswordHasher<User> hasher)
    {
        var hash = hasher.HashPassword(null!, user.Password); // user is not needed here, as not used by default implemetaiton.
        var createdUser = db.Users.Add(new() { Login = user.Login, PasswordHash = hash });
        await db.SaveChangesAsync();
        return Results.RedirectToRoute("home");
    }

    public static async Task<IResult> LoginAsync([FromForm] Dto.AuthUser user, SlistDb db, IPasswordHasher<User> hasher)
    {
        var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Login == user.Login);
        if (existingUser is null) return Results.BadRequest("Unknown user");
        var verificationResult = hasher.VerifyHashedPassword(existingUser, existingUser.PasswordHash, user.Password);
        return verificationResult switch
        {
            PasswordVerificationResult.Success => Results.RedirectToRoute("home"),
            PasswordVerificationResult.Failed => Results.Forbid(),
            PasswordVerificationResult.SuccessRehashNeeded => throw new NotImplementedException("Rehashing is needed but not supported."),
            _ => throw new NotImplementedException($"Password verification result {verificationResult} is not supported."),
        };
    }
}