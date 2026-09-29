using System.Diagnostics;
using DoctypeHtml.Parser;

namespace Serv.Methods;

public static class PageMethods
{
    public static IResult GetRootPage(ComponentProvider componentProvider)
    {
        var folder = "./pages/index";
        var (maybeIndex, maybeError) = componentProvider.GetComponent(folder);
        if (maybeError.HasValue) return Results.InternalServerError();
        Debug.Assert(maybeIndex is not null);
        return Results.Content(File.ReadAllText(maybeIndex.Html), "text/html");
    }
}

public enum ComponentError { MissingHtml, MissingCss, MissingJs }
public sealed record Component(string Html, string Css, string Js);
public class ComponentProvider
{
    public (Component?, ComponentError?) GetComponent(string folder)
    {
        var html = FindFile(folder, "index.html");
        if (html is null) return (null, ComponentError.MissingHtml);
        var css = FindFile(folder, "styles.css");
        if (css is null) return (null, ComponentError.MissingCss);
        var js = FindFile(folder, "script.js");
        if (js is null) return (null, ComponentError.MissingJs);
        return (new(html, css, js), null);
    }

    public string? FindFile(string folder, string name)
    {
        var file = Path.Join(folder, name);
        return File.Exists(file) ? file : null;
    }
}
