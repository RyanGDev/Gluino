using System.Reflection;
using Gluino;

namespace TestApp;

public class AppResourceScheme
{
    public AppResourceScheme(WebView webView)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceDir = $"{assembly.GetName().Name}.wwwroot";
        webView.ResourceRequested += (_, e) => {
            if (!e.Request.Url.StartsWith("app://"))
                return;

            var path = e.Request.Url[6..];
            var resourceName = $"{resourceDir}.{path.Trim('/').Replace('/', '.')}";
            var resource = assembly.GetManifestResourceStream(resourceName);

            e.Response.Content = resource;
            e.Response.ContentType = Path.GetExtension(path) switch {
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "text/javascript",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                ".ico" => "image/x-icon",
                ".json" => "application/json",
                ".woff" => "font/woff",
                ".woff2" => "font/woff2",
                ".ttf" => "font/ttf",
                ".otf" => "font/otf",
                ".eot" => "application/vnd.ms-fontobject",
                ".sfnt" => "application/font-sfnt",
                ".xml" => "text/xml",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
            e.Response.StatusCode = 200;

            TestWindow.LogWebViewEvent($"ResourceRequested: {e.Request.Url}");
        };
    }
}