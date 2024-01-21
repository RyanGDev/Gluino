using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Gluino;

/// <summary>
/// Represents a URL or HTML content source for a WebView.
/// </summary>
public readonly partial struct WebViewSource
{
    [GeneratedRegex("^https?://", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex CreateHttpRegex();
    private static readonly Regex HttpRegex = CreateHttpRegex();

    private WebViewSource(string value, bool isUrl)
    {
        Value = value;
        IsUrl = isUrl;
    }

    /// <summary>
    /// Gets the source URL, file path, or HTML content.
    /// </summary>
    internal string Value { get; }

    /// <summary>
    /// Gets whether the source is a URL.
    /// </summary>
    public bool IsUrl { get; }

    /// <summary>
    /// Create a new <see cref="WebViewSource"/> from a <see cref="Uri"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/>.</param>
    /// <returns>The <see cref="WebViewSource"/> created from the <see cref="Uri"/>.</returns>
    public static WebViewSource FromUri(Uri uri) => new(uri.ToString(), true);

    /// <summary>
    /// Create a new <see cref="WebViewSource"/> from a URL, file, or HTML content.
    /// </summary>
    /// <param name="value">The <see cref="string"/> value.</param>
    /// <returns>The <see cref="WebViewSource"/> created from the <see cref="string"/> value.</returns>
    /// <remarks>
    /// If the value does not match that of a URL or path to a file, it is handled as HTML content.
    /// </remarks>
    public static WebViewSource FromString(string value)
    {
        if (HttpRegex.IsMatch(value))
            return new(value, true);
        
        var path = Path.GetFullPath(value);
        if (File.Exists(path))
            return FromUri(new Uri(path, UriKind.Absolute));

        return new(value, false);
    }

    /// <summary>
    /// Create a new <see cref="WebViewSource"/> from an embedded resource.
    /// </summary>
    /// <param name="name">The name of the embedded resource.</param>
    /// <param name="assembly">
    /// The <see cref="Assembly"/> that contains the embedded resource. If <see langword="null"/>, the entry assembly is used.
    /// </param>
    /// <returns>The <see cref="WebViewSource"/> created from the embedded resource.</returns>
    /// <exception cref="InvalidOperationException">
    /// If the <paramref name="assembly"/> is <see langword="null"/> and the entry assembly cannot be found.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// If the <paramref name="name"/> cannot be found in the <paramref name="assembly"/>.
    /// </exception>
    public static WebViewSource FromResource(string name, Assembly assembly = null)
    {
        assembly ??= Assembly.GetEntryAssembly();
        if (assembly is null)
            throw new InvalidOperationException("Unable to find the entry assembly.");

        var fullName = assembly.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith(name, StringComparison.OrdinalIgnoreCase));
        if (fullName == null)
            throw new FileNotFoundException($"Resource not found: {name}");

        using var resourceStream = assembly.GetManifestResourceStream(fullName);
        return FromStream(resourceStream);
    }

    /// <summary>
    /// Create a new <see cref="WebViewSource"/> from a <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to create the <see cref="WebViewSource"/> from.</param>
    /// <returns>The <see cref="WebViewSource"/> created from the <see cref="Stream"/>.</returns>
    public static WebViewSource FromStream(Stream stream)
    {
        using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
        reader.BaseStream.Seek(0, SeekOrigin.Begin);

        return FromString(reader.ReadToEnd());
    }

    /// <summary>
    /// Create a new <see cref="WebViewSource"/> from a <see cref="byte"/> array.
    /// </summary>
    /// <param name="bytes">The <see cref="byte"/> array to create the <see cref="WebViewSource"/> from.</param>
    /// <returns>The <see cref="WebViewSource"/> created from the <see cref="byte"/> array.</returns>
    public static WebViewSource FromBytes(byte[] bytes) => FromString(Encoding.UTF8.GetString(bytes));

    /// <summary>
    /// Implicit conversion from <see cref="Uri"/>.
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/> to create the <see cref="WebViewSource"/> from.</param>
    public static implicit operator WebViewSource(Uri uri) => FromUri(uri);

    /// <summary>
    /// Implicit conversion from a URL, file, or HTML content.
    /// </summary>
    /// <param name="value">The <see cref="string"/> value.</param>
    public static implicit operator WebViewSource(string value) => FromString(value);

    /// <summary>
    /// Implicit conversion from <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/>.</param>
    public static implicit operator WebViewSource(Stream stream) => FromStream(stream);

    /// <summary>
    /// Implicit conversion from <see cref="byte"/> array.
    /// </summary>
    /// <param name="bytes">The <see cref="byte"/> array.</param>
    public static implicit operator WebViewSource(byte[] bytes) => FromBytes(bytes);

    /// <summary>
    /// Converts the <see cref="WebViewSource"/> to a <see cref="string"/>.
    /// </summary>
    /// <returns>The <see cref="WebViewSource"/> as a <see cref="string"/>.</returns>
    public override string ToString() => Value;
}
