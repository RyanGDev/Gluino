using Gluino;
using TestApp.Properties;

namespace TestApp;

public partial class ChildWindow : Window
{
    public ChildWindow()
    {
        Title = "Child Window";
        Icon = Resources.icon;

        WebView.StartSource = WebViewSource.FromResource("child.html");
        WebView.ContextMenuEnabled = true;
        WebView.DevToolsEnabled = true;

        _ = new AppResourceScheme(WebView);
    }

    [Binding]
    public void CloseWindow()
    {
        Close();
    }
}