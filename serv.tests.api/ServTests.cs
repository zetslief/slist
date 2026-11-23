using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Serv.Db;

namespace Serv.Tests.Api;

public class ServTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory = factory;

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task BasicLoginWorkflow()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            using var db = scope.ServiceProvider.GetRequiredService<SlistDb>();
            await db.Database.EnsureCreatedAsync(CancellationToken);
        }

        using var registerClient = _factory.CreateClient();
        var userContent = new FormUrlEncodedContent([
            new("login", "admin"),
            new("password", "admin"),
        ]);
        var response = await registerClient.PostAsync("/register", userContent, CancellationToken);
        response.EnsureSuccessStatusCode();
        Console.WriteLine(await response.Content.ReadAsStringAsync(CancellationToken));

        using var checkLoginClient = _factory.CreateClient(new() { AllowAutoRedirect = false });
        var loginCheckResponse = await checkLoginClient.GetAsync("/", CancellationToken);
        Assert.Equal(HttpStatusCode.Redirect, loginCheckResponse.StatusCode);
        Assert.Equal("/login", loginCheckResponse.Headers.Location?.AbsolutePath);

        using var loginClient = _factory.CreateClient();
        var loginPostResponse = await loginClient.PostAsync("/login", userContent, CancellationToken);
        Assert.Equal(HttpStatusCode.OK, loginPostResponse.StatusCode);

        var rootResponse = await loginClient.GetAsync("/", CancellationToken);
        Assert.Equal(HttpStatusCode.OK, loginPostResponse.StatusCode);
        Assert.NotEmpty(await rootResponse.Content.ReadAsStringAsync(CancellationToken));
    }
}
