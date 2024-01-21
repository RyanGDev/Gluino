using System.Security.Cryptography;
using Gluino;

namespace SvelteExample;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        Title = "Svelte Example";
        WebView.StartSource = WebViewSource.FromResource("index.html");
        WebView.ContextMenuEnabled = false;
        //WebView.StartSource = "http://localhost:5173";
        //WebView.DevToolsEnabled = true;
    }

    [Binding]
    public static int RandomNumber(int min, int max)
    {
        if (min > max) return max;

        using var rng = RandomNumberGenerator.Create();
        var buffer = new byte[4];
        rng.GetBytes(buffer);
        var value = BitConverter.ToInt32(buffer, 0);
        var normalized = Math.Abs(value / (double)int.MaxValue);
        return (int)(min + (normalized * (max - min)));
    }
}