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
        var (maybeIndex, maybeError) = componentProvider.GetComponent(folder);
        if (maybeError.HasValue) return Results.InternalServerError();
        Debug.Assert(maybeIndex is not null);
        // TODO: fix this mess. better API is needed. either reuse the builer or use a builder factory (lol) or have a better idea.
        (var maybeSlist, maybeError) = componentProvider.GetComponent("./pages/slist"); 
        if (maybeError.HasValue) return Results.InternalServerError();
        Debug.Assert(maybeSlist is not null);
        var listsData = await db.Lists.AsNoTracking().ToArrayAsync();
        var slists = listsData.Select(list => componentProvider.GetComponent("./pages/slist").Value!.Html
            .AddChild("sliststyles", maybeSlist.Css.Build())
            .AddChild("slistscript", maybeSlist.Js.Build())
            .AddChild("slisttitle", ComponentBuilder.Text(list.Title))
            .Build()).ToImmutableArray();
        var component = maybeIndex.Html
            .AddChild("sliststyles", maybeIndex.Css.Build())
            .AddChild("slistscript", maybeIndex.Js.Build())
            .AddChild("slistlists", ComponentBuilder.List(slists))
            .Build();
        return Results.Content(new HtmlTemplate().Render(component), "text/html");
    }

    public static IResult GetLoginPage(ComponentProvider componentProvider)
    {
        var folder = "./pages/login";
        var (maybeIndex, maybeError) = componentProvider.GetComponent(folder);
        if (maybeError.HasValue) return Results.InternalServerError();
        Debug.Assert(maybeIndex is not null);
        var component = maybeIndex.Html
            .AddChild("sliststyles", maybeIndex.Css.Build())
            .AddChild("slistscript", maybeIndex.Js.Build())
            .Build();
        return Results.Content(new HtmlTemplate().Render(component), "text/html");
    }
}

public record PageComponents(ComponentBuilder Html, ComponentBuilder Css, ComponentBuilder Js);

public class ComponentProvider
{
    public Result<PageComponents, IOError> GetComponent(string folder)
    {
        var html = ComponentBuilder.FromHtmlFile(Path.Join(folder, "index.html"));
        if (html.IsError) return Result<PageComponents, IOError>.Fail(html.Error!.Value);
        var css = ComponentBuilder.FromCssFile(Path.Join(folder, "styles.css"));
        if (css.IsError) return Result<PageComponents, IOError>.Fail(css.Error!.Value);
        var js = ComponentBuilder.FromJsFile(Path.Join(folder, "script.js"));
        if (js.IsError) return Result<PageComponents, IOError>.Fail(js.Error!.Value);
        return Result<PageComponents, IOError>.Ok(new(html.Value!, css.Value!, js.Value!));
    }

    public string? FindFile(string folder, string name)
    {
        var file = Path.Join(folder, name);
        return File.Exists(file) ? file : null;
    }
}
