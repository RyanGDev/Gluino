using Gluino.Interop;

namespace Gluino;

/// <summary>
/// Represents the embedded WebView control.
/// </summary>
public partial class WebView
{
    private readonly Window _window;
    private readonly Action _fnInitBindings;

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

    internal WebView(Window window, Action fnInitBindings)
    {
        NativeOptions = new() {
            UserDataPath = App.Options.UserDataPath,
            ContextMenuEnabled = true,
            UserAgent = App.Options.UserAgent
        };

        NativeEvents = new() {
            OnCreated = InvokeCreated,
            OnNavigationStart = InvokeNavigationStart,
            OnNavigationEnd = InvokeNavigationEnd,
            OnMessageReceived = InvokeMessageReceived,
            OnResourceRequested = InvokeResourceRequested
        };

        _window = window;
        _fnInitBindings = fnInitBindings;
    }

    /// <summary>
    /// Gets or sets the user data path for the WebView.
    /// </summary>
    /// <remarks>
    /// Can only be set before the WebView is created.
    /// </remarks>
    public string UserDataPath {
        get => GetUserDataPath();
        set {
            if (InstancePtr != nint.Zero) return;
            NativeOptions.UserDataPath = value;
        }
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
    /// Can only be set before the WebView is created.<br />
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
    
    private void Invoke(Action action) => _window.Invoke(action);
    private void SafeInvoke(Action action) => _window.SafeInvoke(action);
    private T SafeInvoke<T>(Func<T> func) => _window.SafeInvoke(func);
    
    private void InvokeCreated()
    {
        InjectScriptOnDocumentCreated(
            $$"""
            (function () {
              window.gluino.uuid = function () {
                if (crypto.randomUUID) return crypto.randomUUID();
            
                const data = new Uint8Array(16);
                crypto.getRandomValues(data);
                data[6] = (data[6] & 0x0f) | 0x40;
                data[8] = (data[8] & 0x3f) | 0x80;
                const hex = Array.from(data, (byte) => byte.toString(16).padStart(2, '0')).join('');
                return `${hex.substring(0, 8)}-${hex.substring(8, 12)}-4${hex.substring(13, 16)}-${((data[15] & 0x3f) | 0x80)
                  .toString(16)
                  .padStart(2, '0')}${hex.substring(17, 20)}-${hex.substring(20, 32)}`;
              };
            
              const __bindPrefix = 'bind:';
              const __bindCallbacks = {};
            
              window.gluino.addListener(function (e) {
                if (!e.startsWith(__bindPrefix)) return;
                const cbData = JSON.parse(e.slice(__bindPrefix.length));
                if (__bindCallbacks[cbData.id]) {
                  __bindCallbacks[cbData.id](cbData.ret);
                  delete __bindCallbacks[cbData.id];
                }
              });
            
              window.gluino.invoke = function(name, args, cb) {
                const fnData = {
                  id: window.gluino.uuid(),
                  name,
                  args,
                };
                __bindCallbacks[fnData.id] = cb;
                window.gluino.sendMessage(__bindPrefix + JSON.stringify(fnData));
              };
              
              window.gluino.bindings = { {{ToCamelCase(_window.GetType().Name)}}: {} };
            })();
            """);

        _fnInitBindings();

        Created?.Invoke(this, EventArgs.Empty);
    }

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

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.Length == 1)
            return value.ToLowerInvariant();

        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}