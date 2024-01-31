using Gluino;
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

        _ = new AppResourceScheme(WebView);
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

    [Binding]
    public static void OpenChildWindow()
    {
        var childWindow = new ChildWindow();
        childWindow.Show();
    }
    
    private static void LogWindowEvent(string message) => Console.WriteLine(@"[EVENT] [Window] {0}", message);
    internal static void LogWebViewEvent(string message) => Console.WriteLine(@"[EVENT] [WebView] {0}", message);
}