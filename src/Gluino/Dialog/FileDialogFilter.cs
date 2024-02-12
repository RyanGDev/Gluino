using Gluino.Interop;

namespace Gluino;

/// <summary>
/// Represents a file dialog filter.
/// </summary>
public class FileDialogFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileDialogFilter"/> class.
    /// </summary>
    /// <param name="name">The name of the filter.</param>
    /// <param name="extensions">The extensions to filter by.</param>
    public FileDialogFilter(string name, params string[] extensions)
    {
        Name = name;
        foreach (var ext in extensions)
            Extensions.Add(ext);
    }

    /// <summary>
    /// Gets or sets the name of the filter.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets a collection of the extensions for the filter.
    /// </summary>
    public FileDialogFilterExtensionCollection Extensions { get; } = [];

    internal NativeFileDialogFilter ToNativeFilter()
    {
        var filter = new NativeFileDialogFilter {
            Name = Name,
            Spec = string.Join(";", Extensions)
        };

        return filter;
    }
}
