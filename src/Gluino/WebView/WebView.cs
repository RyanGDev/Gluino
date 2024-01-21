using Gluino.Interop;

namespace Gluino;

/// <summary>
/// Represents the embedded WebView control.
/// </summary>
public partial class WebView
{
    private readonly Window _window;
    private readonly WebViewBinder _binder;

    internal nint InstancePtr;
    internal NativeWebViewOptions NativeOptions;
    internal NativeWebViewEvents NativeEvents;

    /// <summary>
    /// Occurs when the WebView is created.
    /// </summary>
    public event EventHandler Created;
    /// <summary>
    /// Occurs when the WebView begins navigating to a new URL.
    /// </summary>
    public event EventHandler<NavigationStartEventArgs> NavigationStart;
    /// <summary>
    /// Occurs when the WebView finishes navigating.
    /// </summary>
    public event EventHandler NavigationEnd;
    /// <summary>
    /// Occurs when the WebView receives a message from the page.
    /// </summary>
    public event EventHandler<string> MessageReceived;
    /// <summary>
    /// Occurs when the WebView requests a resource.
    /// </summary>
    public event EventHandler<WebResourceRequestedEventArgs> ResourceRequested;

    internal WebView(Window window)
    {
        NativeOptions = new() {
            ContextMenuEnabled = true
        };

        NativeEvents = new() {
            OnCreated = InvokeCreated,
            OnNavigationStart = InvokeNavigationStart,
            OnNavigationEnd = InvokeNavigationEnd,
            OnMessageReceived = InvokeMessageReceived,
            OnResourceRequested = InvokeResourceRequested
        };

        _window = window;
        _binder = new WebViewBinder(window, this);
    }
    
    /// <summary>
    /// Sets the source URL or HTML content to load when the WebView is created.
    /// </summary>
    public WebViewSource StartSource {
        set {
            if (value.IsUrl)
                NativeOptions.StartUrl = value.Value;
            else
                NativeOptions.StartContent = value.Value;
        }
    }

    /// <summary>
    /// Gets or sets whether the WebView should display a context menu when right-clicked.
    /// </summary>
    /// <remarks>
    /// Default: true
    /// </remarks>
    public bool ContextMenuEnabled {
        get => GetContextMenuEnabled();
        set => SetContextMenuEnabled(value);
    }

    /// <summary>
    /// Gets or sets whether to enable the WebView's dev tools.
    /// </summary>
    /// <remarks>
    /// Default: false
    /// </remarks>
    public bool DevToolsEnabled {
        get => GetDevToolsEnabled();
        set => SetDevToolsEnabled(value);
    }

    /// <summary>
    /// Gets or sets the user agent string to use when making requests.
    /// </summary>
    public string UserAgent {
        get => GetUserAgent();
        set => SetUserAgent(value);
    }

    /// <summary>
    /// Gets or sets whether the WebView should be granted permissions to access local resources (camera, microphone, etc).
    /// </summary>
    /// <remarks>
    /// Default: false<br />
    /// Windows only.
    /// </remarks>
    public bool GrantPermissions {
        get => GetGrantPermissions();
        set {
            if (InstancePtr != nint.Zero) return;
            NativeOptions.GrantPermissions = value;
        }
    }
    
    /// <summary>
    /// Navigate to the specified <see cref="WebViewSource"/>.
    /// </summary>
    /// <param name="source">The <see cref="WebViewSource"/> to navigate to.</param>
    public void Navigate(WebViewSource source)
    {
        if (InstancePtr == nint.Zero) {
            StartSource = source;
            return;
        }
        
        if (source.IsUrl)
            Invoke(() => NativeWebView.Navigate(InstancePtr, source.Value));
        else
            Invoke(() => NativeWebView.NativateToString(InstancePtr, source.Value));
    }

    /// <summary>
    /// Sends a message to the WebView.
    /// </summary>
    /// <param name="message">The message to send.</param>
    public void SendMessage(string message) => SafeInvoke(() => NativeWebView.PostWebMessage(InstancePtr, message));

    /// <summary>
    /// Injects the specified JavaScript code.
    /// </summary>
    /// <param name="script">The JavaScript code to inject.</param>
    public void InjectScript(string script) => SafeInvoke(() => NativeWebView.InjectScript(InstancePtr, script, false));

    /// <summary>
    /// Injects the specified JavaScript code when the document is created.
    /// </summary>
    /// <param name="script">The JavaScript code to inject.</param>
    public void InjectScriptOnDocumentCreated(string script) => SafeInvoke(() => NativeWebView.InjectScript(InstancePtr, script, true));

    /// <summary>
    /// Bind a C# method to JavaScript.
    /// </summary>
    /// <param name="name">The name of the function that will be created in JavaScript.</param>
    /// <param name="fn">The method to bind.</param>
    /// <param name="global">Whether the function should be created in a global scope.</param>
    /// <remarks>
    /// Example binding:
    /// <example>
    /// <code>
    /// // C#
    /// public class MainWindow : Window
    /// {
    ///     public MainWindow()
    ///     {
    ///         webView.Bind("test", (string arg1, string arg2) => {
    ///             Console.WriteLine($"arg1: {arg1}, arg2: {arg2}"); // arg1: Hello, arg2: World!
    ///             return "Hello from C#!";
    ///         });
    ///     }
    /// }
    /// </code>
    /// <code>
    /// // JavaScript
    /// const result = await window.gluino.bindings.mainWindow.test("Hello", "World!");
    /// cosnole.log(result); // Hello from C#!
    /// </code>
    /// </example>
    /// <br/>
    /// If the <paramref name="global"/> parameter is set to <see langword="false"/>, the binding will be put directly in <c>window.gluino.bindings</c>.<br/>
    /// Example:
    /// <example>
    /// <code>
    /// // JavaScript
    /// const result = await window.gluino.bindings.test("Hello", "World!");
    /// </code>
    /// </example>
    /// </remarks>
    public void Bind(string name, Delegate fn, bool global = false) => _binder.Bind(name, fn, global);

    private void Invoke(Action action) => _window.Invoke(action);
    private void SafeInvoke(Action action) => _window.SafeInvoke(action);
    private T SafeInvoke<T>(Func<T> func) => _window.SafeInvoke(func);
    
    private void InvokeCreated() => Created?.Invoke(this, EventArgs.Empty);
    private void InvokeNavigationStart(string url) => NavigationStart?.Invoke(this, new (url));
    private void InvokeNavigationEnd() => NavigationEnd?.Invoke(this, EventArgs.Empty);
    private void InvokeMessageReceived(string message) => MessageReceived?.Invoke(this, message);

    private void InvokeResourceRequested(NativeWebResourceRequest request, out NativeWebResourceResponse response)
    {
        var req = new WebResourceRequest(request);
        var res = new WebResourceResponse();

        ResourceRequested?.Invoke(this, new(req, res));

        response = res.Native;
    }
}