using System.Collections.Immutable;
using System.Diagnostics;
using HtmlT;

using Microsoft.EntityFrameworkCore;

using Serv.Db;

namespace Serv.Methods;

public static class PageMethods
{
    public static async Task<IResult> GetRootPage(SlistDb db, ComponentProvider componentProvider)
    {
        var folder = "./pages/index";
        var indexComponents = componentProvider.GetComponent(folder);
        var slistComponents = componentProvider.GetComponent("./pages/slist");
        var listsData = await db.Lists.AsNoTracking().ToArrayAsync();
        var slists = listsData.Select(list => ComponentBuilder.FromHtml(slistComponents.Html)
            .AddChild("slisttitle", ComponentBuilder.Text(list.Title))
            .AddData("SLIST_ID", list.Id.ToString())
            .Build()).ToImmutableArray();
        var component = ComponentBuilder.FromHtml(indexComponents.Html)
            .AddChild("sliststyles", ComponentBuilder.FromCss(indexComponents.Css + slistComponents.Css).Build())
            .AddChild("slistscript", ComponentBuilder.FromJs(indexComponents.Js + slistComponents.Js).Build())
            .AddChild("slistlists", ComponentBuilder.List(slists))
            .Build();
        return Results.Content(new HtmlTemplate().Render(component), "text/html");
    }

    public static IResult GetLoginPage(ComponentProvider componentProvider)
    {
        var folder = "./pages/login";
        var loginComponents = componentProvider.GetComponent(folder);
        var component = ComponentBuilder.FromHtml(loginComponents.Html)
            .AddChild("sliststyles", ComponentBuilder.FromCss(loginComponents.Css).Build())
            .AddChild("slistscript", ComponentBuilder.FromJs(loginComponents.Js).Build())
            .Build();
        return Results.Content(new HtmlTemplate().Render(component), "text/html");
    }
}

public record PageComponents(string Html, string Css, string Js);

public class ComponentProvider
{
    public PageComponents GetComponent(string folder)
    {
        var html = File.ReadAllText(Path.Join(folder, "index.html"));
        var css = File.ReadAllText(Path.Join(folder, "styles.css"));
        var js = File.ReadAllText(Path.Join(folder, "script.js"));
        return new(html, css, js);
    }

    public string? FindFile(string folder, string name)
    {
        var file = Path.Join(folder, name);
        return File.Exists(file) ? file : null;
    }
}
