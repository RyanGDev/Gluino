using System.Collections;

namespace Gluino;

/// <summary>
/// Represents a collection of file dialog filters.
/// </summary>
public class FileDialogFilterCollection : ICollection<FileDialogFilter>
{
    private readonly List<FileDialogFilter> _items = [];

    /// <summary>
    /// Gets or sets the filter at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the filter to get or set.</param>
    /// <returns>The filter at the specified index.</returns>
    public FileDialogFilter this[int index] {
        get => _items[index];
        set => _items[index] = value;
    }

    /// <summary>
    /// Gets the number of filters in the collection.
    /// </summary>
    public int Count => _items.Count;

    /// <inheritdoc />
    public IEnumerator<FileDialogFilter> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Add a filter to the collection.
    /// </summary>
    /// <param name="item">The filter to add.</param>
    public void Add(FileDialogFilter item) => _items.Add(item);

    /// <summary>
    /// Add a filter to the collection.
    /// </summary>
    /// <param name="name">The name of the filter.</param>
    /// <param name="extensions">The extensions to filter by.</param>
    /// <returns>The <see cref="FileDialogFilter"/> that was added to the collection.</returns>
    public FileDialogFilter Add(string name, params string[] extensions)
    {
        var filter = new FileDialogFilter(name, extensions);
        Add(filter);
        return filter;
    }

    /// <summary>
    /// Removes the first occurrence of a specific filter from the collection.
    /// </summary>
    public void Clear() => _items.Clear();

    /// <summary>
    /// Determines whether the collection contains a specific filter.
    /// </summary>
    /// <param name="item">The filter to locate in the collection.</param>
    /// <returns>
    /// <see langword="true" /> if the filter is found in the collection; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(FileDialogFilter item) => _items.Contains(item);

    /// <summary>
    /// Copies the elements of the collection to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from the collection.
    /// </param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
    public void CopyTo(FileDialogFilter[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    /// <summary>
    /// Removes the first occurrence of a specific filter from the collection.
    /// </summary>
    /// <param name="item">The filter to remove from the collection.</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="item"/> was successfully removed from the collection; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Remove(FileDialogFilter item) => _items.Remove(item);
    
    bool ICollection<FileDialogFilter>.IsReadOnly => false;
}
