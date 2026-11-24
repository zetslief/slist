using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Serv.Db;

namespace Serv.Tests.Api;

public class ServTests(WebApplicationFactory<Program> factory, ITestOutputHelper output) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory = factory;

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
    private static Dto.AuthUser Admin { get; } = new("admin", "admin");

    [Fact]
    public async Task BasicRegistrationAndLogin()
    {
        await EnsureDatabaseCreatedAsync();

        using var registerClient = _factory.CreateClient(new() { AllowAutoRedirect = false });
        var response = await RegisterAsync(registerClient, Admin);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.AbsolutePath);

        using var loginClient = _factory.CreateClient();
        var loginPostResponse = await LoginAsync(loginClient, Admin);
        Assert.Equal(HttpStatusCode.OK, loginPostResponse.StatusCode);

        var rootResponse = await loginClient.GetAsync("/", CancellationToken);
        Assert.Equal(HttpStatusCode.OK, loginPostResponse.StatusCode);
        Assert.NotEmpty(await rootResponse.Content.ReadAsStringAsync(CancellationToken));
        Assert.Equal("text/html", rootResponse.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task CreateList()
    {
        await EnsureDatabaseCreatedAsync();

        using var client = _factory.CreateClient();
        var registerResponse = await RegisterAsync(client, Admin);
        registerResponse.EnsureSuccessStatusCode();

        var createResponse = await client.PostAsJsonAsync("/lists/new", new Dto.NewList("Test new list"), CancellationToken);
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        Assert.Equal("application/json", createResponse.Content.Headers.ContentType?.MediaType);
        var createdList = await createResponse.Content.ReadFromJsonAsync<Dto.List>(CancellationToken);
        Assert.NotNull(createdList);
        Assert.NotEqual(Guid.Empty, createdList.Id);
        Assert.Equal("Test new list", createdList.Title);
    }

    private async Task<HttpResponseMessage> RegisterAsync(HttpClient client, Dto.AuthUser user)
    {
        var response = await client.PostAsync("/register", CreateFormDataFromUser(user), CancellationToken);
        output.WriteLine($"Reigseter: {response}");
        return response;
    }
    
    private async Task<HttpResponseMessage> LoginAsync(HttpClient client, Dto.AuthUser user)
    {
        var response = await client.PostAsync("/login", CreateFormDataFromUser(user), CancellationToken);
        output.WriteLine($"Login: {response}");
        return response;
    }

    private async Task EnsureDatabaseCreatedAsync()
    {
        using var scope = _factory.Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<SlistDb>();
        await db.Database.EnsureCreatedAsync(CancellationToken);
    }

    private static FormUrlEncodedContent CreateFormDataFromUser(Dto.AuthUser user) => new([
        new("login", user.Login),
        new("password", user.Password),
    ]);
}
