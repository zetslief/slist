using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Serv.Tests.Api;

public class ServTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory = factory;

    [Fact]
    public async Task Test1()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var loginContent = new FormUrlEncodedContent([
            new("login", "admin"),
            new("password", "admin"),
        ]);
        var response = await client.PostAsync("/login", loginContent, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        Console.WriteLine(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var rootResponse = await client.GetAsync("/", TestContext.Current.CancellationToken);
        rootResponse.EnsureSuccessStatusCode();
        var content = await rootResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    }
}
