namespace NorthEdge.Utilities;

/// <summary>
/// Extension class used to resize a generic list
/// </summary>
public static class ListExtension
{
    /// <summary>
    /// Resizes the current list to the specified size using a functor to initialize new values.
    /// </summary>
    /// <param name="list">the list to be resized</param>
    /// <param name="size">the new size of the list</param>
    /// <param name="newElement">the functor used to initialize new values</param>
    /// <typeparam name="T">the type of the elements of the list</typeparam>
    private static void Resize<T>(this List<T> list, int size, Func<T> newElement)
    {
        var count = list.Count;

        if (size < count)
        {
            list.RemoveRange(size, count - size);
        }
        else if (size > count)
        {
            // don't use AddRange or the new elements might end up being all the same reference
            for (var j = 0; j < size - count; ++j)
            {
                list.Add(newElement());
            }
        }
    }

    /// <summary>
    /// Resizes the current list to the specified size using the
    /// constructor of the generic type to initialize new values.
    /// </summary>
    /// <param name="list">the list to be resized</param>
    /// <param name="size">the new size of the list</param>
    /// <typeparam name="T">the type of the elements of the list</typeparam>
    public static void Resize<T>(this List<T> list, int size) where T: new()
    {
        Resize(list, size, () => new T());
    }

    /// <summary>
    /// Resizes the current list to the specified size using a default value to initialize new values.
    /// </summary>
    /// <param name="list">the list to be resized</param>
    /// <param name="size">the new size of the list</param>
    /// <param name="element">the default value for new elements of the list</param>
    /// <typeparam name="T">the type of the elements of the list</typeparam>
    public static void Resize<T>(this List<T> list, int size, T element)
    {
        Resize(list, size, () => element);
    }
}