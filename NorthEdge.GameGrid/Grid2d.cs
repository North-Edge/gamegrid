using NorthEdge.Utilities;
using System.Collections;
using System.Text;

namespace NorthEdge.GameGrid;

/// <summary>
/// A 2D grid of generic elements
/// </summary>
/// <typeparam name="T">the type of the elements in the <see cref="Grid2d{T}"/></typeparam>
public class Grid2d<T>(): IEnumerable<IList<T>>
{
    #region Properties
    /// <summary>
    /// The elements of the <see cref="Grid2d{T}"/> as list of columns containing a list of rows
    /// </summary>
    private readonly List<List<T>> _elements = [];
    /// <summary>
    /// A functor used to clamp the value of the elements of the <see cref="Grid2d{T}"/> when they are modified
    /// </summary>
    private Func<T, T>? _clampFunc;
    /// <summary>
    /// The number of rows in the <see cref="Grid2d{T}"/>
    /// </summary>
    public int Rows => _elements.Count;
    /// <summary>
    /// The number of columns in the <see cref="Grid2d{T}"/>
    /// </summary>
    public int Columns => _elements.Count == 0 ? 0 : _elements[0].Count;
    #endregion

    #region Constructor
    
    /// <summary>
    /// The <see cref="Grid2d{T}"/> constructor
    /// </summary>
    /// <param name="rows">the number of rows in the <see cref="Grid2d{T}"/> (must be > 1)</param>
    /// <param name="columns">the number of columns in the <see cref="Grid2d{T}"/> (must be > 1)</param>
    /// <param name="value">the default value for the elements of the <see cref="Grid2d{T}"/></param>
    /// <param name="clampFunc">the optional functor used to clamp the value of the elements of the <see cref="Grid2d{T}"/></param>
    public Grid2d(int rows, int columns, T value = default!, Func<T,T>? clampFunc = null): this()
    {
        _clampFunc = clampFunc;
        Resize(rows, columns, value);
    }

    /// <summary>
    /// Resizes the <see cref="Grid2d{T}"/> to the specified dimensions
    /// </summary>
    /// <param name="rows">the new number of rows in the <see cref="Grid2d{T}"/> (must be > 1)</param>
    /// <param name="columns">the new number of columns in the <see cref="Grid2d{T}"/> (must be > 1)</param>
    /// <param name="value">the default value for the elements of the <see cref="Grid2d{T}"/></param>
    /// <exception cref="ArgumentOutOfRangeException">The dimensions must be greater than 0</exception>
    public void Resize(int rows, int columns, T value = default!)
    {
        if (rows <= 0)
            throw new ArgumentOutOfRangeException(nameof(rows), "The argument must be greater than 0");
        if (columns <= 0)
            throw new ArgumentOutOfRangeException(nameof(columns), "The argument must be greater than 0");

        _elements.Resize(rows);

        foreach (var gridColumns in _elements)
        {
            gridColumns.Resize(columns, _clampFunc != null ? _clampFunc(value) : value);
        }
    }
    
    #endregion
    
    #region Clamping
    
    /// <summary>
    /// Sets the clamping functor used to clamp the value of the elements of the <see cref="Grid2d{T}"/>
    /// </summary>
    /// <param name="clampFunc">the clamping functor used to clamp the values</param>
    /// <param name="applyNow">flag specifying if the clamping should be applied</param>
    /// <returns>the current <see cref="Grid2d{T}"/></returns>
    public Grid2d<T> SetClamp(Func<T, T>? clampFunc, bool applyNow)
    {
        _clampFunc = clampFunc;

        return _clampFunc != null && applyNow ? ClampValues(_clampFunc) : this;
    }
    
    /// <summary>
    /// Returns a temporary enumerable of the elements of the <see cref="Grid2d{T}"/>
    /// clamped by the specified clamping functor (the elements are not modified).
    /// </summary>
    /// <param name="clampFunc">the clamping functor used to clamp the values</param>
    /// <returns>the temporary enumerable of clamped values</returns>
    public IEnumerable<T> ClampedValues(Func<T,T> clampFunc)
    {
        return Elements().Select(clampFunc);
    }
    
    /// <summary>
    /// Clamps the value of the elements of the <see cref="Grid2d{T}"/> using the specified clamping functor.
    /// </summary>
    /// <param name="clampFunc">the clamping functor used to clamp the values</param>
    /// <returns>the current <see cref="Grid2d{T}"/></returns>
    public Grid2d<T> ClampValues(Func<T,T> clampFunc)
    {
        return Traverse((i, j, element) => _elements[i][j] = clampFunc(element));
    }

    #endregion
    
    #region Accessors
    
    /// <summary>
    /// Grid indexers by row and column index [<paramref name="j"/>,<paramref name="i"/>]
    /// </summary>
    /// <param name="i">the row index of the value to access</param>
    /// <param name="j">the column index of the value to access</param>
    public T this[int i, int j]
    {
        get => GetAt(i, j);
        set => SetAt(i, j, value);
    }
    
    /// <summary>
    /// Retrieves the value of the element at the specified row and column index
    /// </summary>
    /// <param name="i">the row index of the value to retrieve</param>
    /// <param name="j">the column index of the value to retrieve</param>
    /// <returns>the value of the element</returns>
    public T GetAt(int i, int j)
    {
        return Accessor(i, j, element => element);
    }

    /// <summary>
    /// Sets the value of the element at the specified row and column index
    /// </summary>
    /// <param name="i">the row index of the value to set</param>
    /// <param name="j">the column index of the value to set</param>
    /// <param name="value">the new value of the element</param>
    public Grid2d<T> SetAt(int i, int j, T value)
    {
        var clampedValue = _clampFunc != null ? _clampFunc(value) : value;
        
        Accessor(i, j, _ => _elements[i][j] = clampedValue);

        return this;
    }

    /// <summary>
    /// Internal accessor to get or set the value of the element at the specified row and column index.
    /// </summary>
    /// <param name="i">the row index of the value to set</param>
    /// <param name="j">the column index of the value to set</param>
    /// <param name="accessor">functor used to get or set the value of the element</param>
    /// <returns>the value of the element</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private T Accessor(int i, int j, Func<T, T> accessor)
    {
        if (i < _elements.Count)
        {
            var row = _elements[i];

            if (j < row.Count)
            {
                return accessor(_elements[i][j]);
            }

            throw new ArgumentOutOfRangeException(nameof(j));
        }

        throw new ArgumentOutOfRangeException(nameof(i));
    }

    #endregion
    
    #region Iterators
    
    /// <summary>
    /// Iterates on all the elements of the <see cref="Grid2d{T}"/>.
    /// </summary>
    /// <returns>enumerable of all the elements of the <see cref="Grid2d{T}"/></returns>
    public IEnumerable<T> Elements()
    {
        return _elements.SelectMany(rows => rows);
    }

    /// <summary>
    /// Iterates on all the elements of the <see cref="Grid2d{T}"/> and apply the specified transform functor receiving
    /// the row, column and value as its parameters; returning temporary enumerable of the resulting elements.
    /// </summary>
    /// <returns>enumerable of the transformed values</returns>
    public IEnumerable<TResult> Transform<TResult>(Func<int, int, T, TResult> transformFunc)
    {
        return Iterate().Select(t => transformFunc(t.i, t.j, t.element));
    }

    /// <summary>
    /// Internal iterator on all the elements of the <see cref="Grid2d{T}"/> as a struct containing (row, column, value) 
    /// </summary>
    /// <returns>enumerable of the structs containing (row, column, value)</returns>
    private IEnumerable<(int i, int j, T element)> Iterate()
    {
        for(var i = 0; i < _elements.Count; ++i)
        {
            for (var j = 0; j < _elements[i].Count; j++)
            {
                yield return (i, j, _elements[i][j]);
            }
        }
    }

    /// <summary>
    /// Traverses all the elements of the <see cref="Grid2d{T}"/> and applies the
    /// specified action receiving the row, column and value as its parameters. 
    /// </summary>
    /// <param name="action">the action to apply on all the elements</param>
    /// <returns>the current <see cref="Grid2d{T}"/></returns>
    public Grid2d<T> Traverse(Action<int, int, T> action)
    {
        foreach (var tuple in Iterate())
        {
            action(tuple.i, tuple.j, tuple.element);
        }

        return this;
    }
    
    #endregion

    #region Dictionaries
    
    /// <summary>
    /// Transforms the grid into a dictionary of values keyed by their coordinates
    /// </summary>
    /// <returns>a dictionary of values keyed by their coordinates</returns>
    public IDictionary<(int i, int j), T> ToDictionary()
    {
        return Transform((i, j, element) => new KeyValuePair<(int i, int j), T>((i, j), element)).ToDictionary();
    }

    /// <summary>
    /// Transforms the grid into a dictionary containing a list of coordinates of items keyed by their corresponding value.
    /// </summary>
    /// <remarks>This method should NOT be called if the type of the elements is <see cref="Nullable"/>.</remarks>
    /// <returns>a dictionary containing a list of coordinates of items keyed by their corresponding value</returns>
    /// <exception cref="InvalidOperationException">A nullable type cannot be used as a dictionary key.</exception>
    public IDictionary<T, IList<(int i, int j)>> ToCoordinates()
    {
        if (Nullable.GetUnderlyingType(typeof(T)) != null)
            throw new InvalidOperationException("A nullable type cannot be used as a dictionary key");

#pragma warning disable CS8714 // disabled because the case is checked above
        var result = new Dictionary<T, IList<(int i, int j)>>();
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.

        foreach(var tuple in Iterate())
        {
            var value = (tuple.i, tuple.j);

            if (result.TryGetValue(tuple.element, out var list))
            {
                list.Add(value);
            }
            else
            {
                result[tuple.element] = [value];
            }
        }

        return result;
    }
    
    #endregion

    #region object overrides

    /// <inheritdoc cref="object"/>
    public override string ToString()
    {
        return ToString(element => element);
    }

    /// <summary>
    /// Outputs the grid as a string in a grid format
    /// </summary>
    /// <param name="transformFunc">a functor used to transform the value before printing it</param>
    /// <param name="margin">the margin between each column </param>
    /// <typeparam name="TElement"></typeparam>
    /// <returns></returns>
    public string ToString<TElement>(Func<T,TElement> transformFunc, int margin = 1)
    {
        var r = Rows.ToString().Length + 2; // +2 => []
        var c = Columns.ToString().Length;
        var sb = new StringBuilder();
        var maxWidth = c + 2;               // +2 => []
        // clamp the margin to be positive
        margin = Math.Max(margin, 0);
        // determine the width of the longest element
        Traverse((_, _, element) => {
            var elementStr = transformFunc(element)?.ToString() ?? "null";
    
            maxWidth = Math.Max(maxWidth, elementStr.Length);
        });
        // add the padding for the height before printing the header
        sb.Append(new string(' ', r));
        // print the header consisting of all the column indexes
        for (var j = 0; j < Columns; ++j)
            sb.Append($"|{j}|".PadLeft(maxWidth + margin));
        // print all the element of the grid
        Traverse((i, j, element) => {
            var elementStr = transformFunc(element)?.ToString() ?? "null";
            // print the row index at the beginning of each row
            if (j == 0)
                sb.Append('\n' + $"[{i}]".PadLeft(r));
            // print the element
            sb.Append(elementStr.PadLeft(maxWidth + margin));
        });

        return sb.ToString();
    }

    #endregion

    #region IEnumerable implementation

    /// <inheritdoc cref="IEnumerable{T}"/>
    IEnumerator<IList<T>> IEnumerable<IList<T>>.GetEnumerator()
    {
        return _elements.Cast<IList<T>>().GetEnumerator();
    }

    /// <inheritdoc cref="IEnumerable{T}"/>
    public IEnumerator GetEnumerator()
    {
        return _elements.GetEnumerator();
    }

    #endregion
}