using Gluino;
using System.Reflection;
using TestApp.Properties;

namespace TestApp;

public partial class TestWindow : Window
{
    public TestWindow()
    {
        Title = "Test Window";
        Icon = Resources.icon;
        //BorderStyle = WindowBorderStyle.SizableNoCaption;
        //WindowState = WindowState.Maximized;
        //Theme = WindowTheme.System;
        //Resizable = true;
        //TopMost = false;
        //StartupLocation = WindowStartupLocation.Default;
        //MinimizeEnabled = true;
        //MaximizeEnabled = false;

        var assembly = Assembly.GetExecutingAssembly();

        //WebView.StartSource = "https://google.com";
        WebView.StartSource = WebViewSource.FromResource("index.html");
        WebView.ContextMenuEnabled = true;
        WebView.DevToolsEnabled = true;

        Creating += (_, _) => LogWindowEvent("Creating");
        Created += (_, _) => LogWindowEvent("Created");
        Shown += (_, _) => LogWindowEvent("Shown");
        Hidden += (_, _) => LogWindowEvent("Hidden");
        Resize += (_, e) => LogWindowEvent($"Resize: {e.Width}, {e.Height}");
        ResizeStart += (_, e) => LogWindowEvent($"ResizeStart: {e.Width}, {e.Height}");
        ResizeEnd += (_, e) => LogWindowEvent($"ResizeEnd: {e.Width}, {e.Height}");
        LocationChanged += (_, e) => LogWindowEvent($"LocationChanged: {e.X}, {e.Y}");
        WindowStateChanged += (_, e) => LogWindowEvent($"WindowStateChanged: {e.WindowState}");
        FocusIn += (_, _) => {
            WebView.SendMessage("window-focus-in");

            LogWindowEvent("FocusIn");
        };
        FocusOut += (_, _) => {
            WebView.SendMessage("window-focus-out");

            LogWindowEvent("FocusOut");
        };
        Closing += (_, _) => LogWindowEvent("Closing");
        Closed += (_, _) => LogWindowEvent("Closed");

        WebView.Created += (_, _) => LogWebViewEvent("Created");
        WebView.NavigationStart += (_, e) => LogWebViewEvent($"NavigationStart: {e.Url}");
        WebView.NavigationEnd += (_, _) => LogWebViewEvent("NavigationEnd");
        WebView.MessageReceived += (_, e) => LogWebViewEvent($"MessageReceived: {e}");

        var resourceDir = $"{assembly.GetName().Name}.wwwroot";
        WebView.ResourceRequested += (_, e) => {
            if (!e.Request.Url.StartsWith("app://")) {
                return;
            }

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

            LogWebViewEvent($"ResourceRequested: {e.Request.Url}");
        };
    }

    [Binding]
    public static string Test(string arg1, string arg2)
    {
        Console.WriteLine($@"[METHOD] [WebView] Test: {arg1}, {arg2}");
        return "Test result";
    }

    [Binding]
    public static async Task<string> TestAsync(string arg1, string arg2)
    {
        Console.WriteLine($@"[METHOD] [WebView] TestAsync: {arg1}, {arg2}");
        await Task.Delay(2000);
        return await Task.FromResult("Async test result");
    }

    private static void LogWindowEvent(string message) => Console.WriteLine(@"[EVENT] [Window] {0}", message);
    private static void LogWebViewEvent(string message) => Console.WriteLine(@"[EVENT] [WebView] {0}", message);
}
