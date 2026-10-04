using System.Diagnostics;
using HtmlT;

namespace Serv.Methods;

public static class PageMethods
{
    public static IResult GetRootPage(ComponentProvider componentProvider)
    {
        var folder = "./pages/index";
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
