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
    /// <param name="patterns">The patterns to filter by.</param>
    public FileDialogFilter(string name, params string[] patterns)
    {
        Name = name;
        foreach (var ext in patterns)
            Patterns.Add(ext);
    }

    /// <summary>
    /// Gets or sets the name of the filter.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets a collection of patterns for the filter.
    /// </summary>
    public FileDialogFilterPatternCollection Patterns { get; } = [];

    internal NativeFileDialogFilter ToNativeFilter()
    {
        var filter = new NativeFileDialogFilter {
            Name = Name,
            Spec = string.Join(";", Patterns)
        };

        return filter;
    }
}
