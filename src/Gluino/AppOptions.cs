namespace Gluino;

/// <summary>
/// Global application options
/// </summary>
public class AppOptions
{
    /// <summary>
    /// The global default user data path for new webview instances to use.<br/>
    /// <see cref="WebView.UserDataPath"/>
    /// </summary>
    public string UserDataPath { get; set; }

    /// <summary>
    /// The global default user agent for new webview instances to use.<br/>
    /// <see cref="WebView.UserAgent"/>
    /// </summary>
    public string UserAgent { get; set; }
}