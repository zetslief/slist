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
    public async Task List_Create()
    {
        await EnsureDatabaseCreatedAsync();

        using var client = _factory.CreateClient();
        var registerResponse = await RegisterAsync(client, Admin);
        registerResponse.EnsureSuccessStatusCode();

        var createdList = await CreateListAsync(client, new("New List"));
        Assert.NotEqual(Guid.Empty, createdList.Id);
        Assert.Equal("New List", createdList.Title);
    }

    [Fact]
    public async Task List_Rename()
    {
        await EnsureDatabaseCreatedAsync();

        using var client = _factory.CreateClient();
        (await RegisterAsync(client, Admin)).EnsureSuccessStatusCode();

        var list = await CreateListAsync(client, new("List"));

        var renameResponse = await client.PutAsJsonAsync($"/lists/{list.Id}", new Dto.ListUpdate("New List Name"), CancellationToken);
        Assert.Equal(HttpStatusCode.OK, renameResponse.StatusCode);
        Assert.Equal("application/json", renameResponse.Content.Headers.ContentType?.MediaType);
        var renamedList = await renameResponse.Content.ReadFromJsonAsync<Dto.List>(CancellationToken);
        Assert.NotNull(renamedList);
        Assert.Equal(list.Id, renamedList.Id);
        Assert.NotEqual(list.Title, renamedList.Title);

        var unknownListResponse = await client.PutAsJsonAsync($"/lists/{Guid.NewGuid()}", new Dto.ListUpdate("Invalid"), CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, unknownListResponse.StatusCode);
    }

    [Fact]
    public async Task List_Delete()
    {
        await EnsureDatabaseCreatedAsync();

        using var client = _factory.CreateClient();
        (await RegisterAsync(client, Admin)).EnsureSuccessStatusCode();

        var list = await CreateListAsync(client, new("List"));
        var deletedResponse = await client.DeleteAsync($"/lists/{list.Id}", CancellationToken);
        Assert.Equal(HttpStatusCode.OK, deletedResponse.StatusCode);

        var unknownListDeleteResponse = await client.DeleteAsync($"/lists/{Guid.NewGuid()}", CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, unknownListDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task List_AddItem()
    {
        await EnsureDatabaseCreatedAsync();

        using var client = _factory.CreateClient();
        (await RegisterAsync(client, Admin)).EnsureSuccessStatusCode();

        var list = await CreateListAsync(client, new("Test"));

        var newItem = await CreateItemAsync(client, list, new(list.Id, "Item"));
        Assert.NotEqual(Guid.Empty, newItem.Id);
        Assert.Equal("Item", newItem.Name);

        var unknownListResponse = await client.PostAsJsonAsync("/items/new", new Dto.NewItem(Guid.NewGuid(), "Not an item"), CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, unknownListResponse.StatusCode);
    }

    [Fact]
    public async Task List_ToggleItem()
    {
        await EnsureDatabaseCreatedAsync();

        using var client = _factory.CreateClient();
        (await RegisterAsync(client, Admin)).EnsureSuccessStatusCode();

        var list = await CreateListAsync(client, new("List"));
        var newItem = await CreateItemAsync(client, list, new(list.Id, "Item"));

        var toggleResponse = await client.PostAsync($"/items/{newItem.Id}/toggle", null, CancellationToken);
        Assert.Equal(HttpStatusCode.OK, toggleResponse.StatusCode);
        Assert.Equal("application/json", toggleResponse.Content.Headers.ContentType?.MediaType);
        var toggledItem = await toggleResponse.Content.ReadFromJsonAsync<Dto.Item>(CancellationToken);
        Assert.NotNull(toggledItem);
        Assert.Equal(newItem.Id, toggledItem.Id);
        Assert.Equal(newItem.Name, toggledItem.Name);
        Assert.NotEqual(newItem.Checked, toggledItem.Checked);

        var unknownItemResponse = await client.PostAsync($"/items/{Guid.NewGuid()}/toggle", null, CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, unknownItemResponse.StatusCode);
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

    private async Task<Dto.List> CreateListAsync(HttpClient client, Dto.NewList newList)
    {
        var createResponse = await client.PostAsJsonAsync("/lists/new", newList, CancellationToken);
        output.WriteLine(createResponse.ToString());
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        Assert.Equal("application/json", createResponse.Content.Headers.ContentType?.MediaType);
        return await createResponse.Content.ReadFromJsonAsync<Dto.List>(CancellationToken)
            ?? throw new InvalidOperationException($"Failed to read payload.");
    }

    public async Task<Dto.Item> CreateItemAsync(HttpClient client, Dto.List list, Dto.NewItem newItem)
    {
        var newItemResponse = await client.PostAsJsonAsync("/items/new", new Dto.NewItem(list.Id, "Item"), CancellationToken);
        Assert.Equal(HttpStatusCode.OK, newItemResponse.StatusCode);
        Assert.Equal("application/json", newItemResponse.Content.Headers.ContentType?.MediaType);
        return await newItemResponse.Content.ReadFromJsonAsync<Dto.Item>(CancellationToken)
            ?? throw new InvalidOperationException($"Failed to read payload.");
    }

    private async Task EnsureDatabaseCreatedAsync()
    {
        using var scope = _factory.Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<SlistDb>();
        await db.Database.EnsureDeletedAsync(CancellationToken);
        await db.Database.EnsureCreatedAsync(CancellationToken);
    }

    private static FormUrlEncodedContent CreateFormDataFromUser(Dto.AuthUser user) => new([
        new("login", user.Login),
        new("password", user.Password),
    ]);
}
