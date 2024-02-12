using System.Collections;

namespace Gluino;

/// <summary>
/// Represents a collection of file dialog filter extensions.
/// </summary>
public class FileDialogFilterExtensionCollection : ICollection<string>
{
    internal readonly List<string> Items = [];
    
    /// <summary>
    /// Gets or sets the extension at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the extension to get or set.</param>
    public string this[int index] {
        get => Items[index];
        set => Items[index] = Normalize(value);
    }

    /// <summary>
    /// Gets the number of extensions in the collection.
    /// </summary>
    public int Count => Items.Count;

    /// <inheritdoc />
    public IEnumerator<string> GetEnumerator() => Items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Add an extension to the collection.
    /// </summary>
    /// <param name="ext">The extension to add.</param>
    public void Add(string ext)
    {
        ArgumentNullException.ThrowIfNull(ext, nameof(ext));
        Items.Add(Normalize(ext));
    }

    /// <summary>
    /// Removes all extensions from the collection.
    /// </summary>
    public void Clear() => Items.Clear();

    /// <summary>
    /// Determines whether the collection contains a specific extension.
    /// </summary>
    /// <param name="ext">The extension to locate in the collection.</param>
    /// <returns><see langword="true" /> if the extension is found in the collection; otherwise, <see langword="false"/>.</returns>
    public bool Contains(string ext) => Items.Contains(Normalize(ext));

    /// <summary>
    /// Copies the elements of the collection to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied from the collection.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
    public void CopyTo(string[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array, nameof(array));
        Items.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Removes the first occurrence of a specific extension from the collection.
    /// </summary>
    /// <param name="item">The extension to remove from the collection.</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="item"/> was successfully removed from the collection; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Remove(string item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));
        return Items.Remove(Normalize(item));
    }

    /// <summary>
    /// Determines the index of a specific extension in the collection.
    /// </summary>
    /// <param name="item">The extension to locate in the collection.</param>
    /// <returns>The index of <paramref name="item"/> if found in the collection; otherwise, -1.</returns>
    public int IndexOf(string item) => Items.IndexOf(Normalize(item));

    /// <summary>
    /// Inserts an extension into the collection at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
    /// <param name="item">The extension to insert.</param>
    public void Insert(int index, string item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));
        Items.Insert(index, Normalize(item));
    }

    /// <summary>
    /// Removes the extension at the specified index of the collection.
    /// </summary>
    /// <param name="index">The zero-based index of the extension to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.
    /// </exception>
    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Items.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        Items.RemoveAt(index);
    }

    bool ICollection<string>.IsReadOnly => false;

    private static string Normalize(string ext)
    {
        ext = ext.Trim();

        return ext switch {
            "*" => "*.*",
            "*." => "*.*",
            _ when ext.StartsWith("*.") => ext,
            _ when ext.StartsWith('.') => $"*{ext}",
            _ when ext.StartsWith('*') => $"*.{ext[1..]}",
            _ => $"*.{ext}"
        };
    }
}
