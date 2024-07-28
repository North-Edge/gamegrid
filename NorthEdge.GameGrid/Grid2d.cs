using NorthEdge.Utilities;
using System.Collections;
using System.Text;

namespace NorthEdge.GameGrid;

/// <summary>
/// A 2D grid of generic elements
/// </summary>
/// <typeparam name="T">the type of the elements in the <see cref="Grid2d{T}"/></typeparam>
public class Grid2d<T>(): IEnumerable<IList<T>>, IEquatable<Grid2d<T>>
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
    /// <summary>
    /// Default list of indexes to check when evaluating neighbours
    /// </summary>
    protected readonly IList<(int i, int j)> Neighbours = [
        // vertical
        (-1,  0),
        ( 1,  0),
        // horizontal
        ( 0, -1),
        ( 0,  1),
        // diagonals
        (-1,  1),
        ( 1,  1),
        (-1, -1),
        ( 1, -1)
    ];

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
    /// <param name="rowCount">the new number of rows in the <see cref="Grid2d{T}"/> (must be > 1)</param>
    /// <param name="columnCount">the new number of columns in the <see cref="Grid2d{T}"/> (must be > 1)</param>
    /// <param name="value">the default value for the elements of the <see cref="Grid2d{T}"/></param>
    /// <returns>the current <see cref="Grid2d{T}"/></returns>
    /// <exception cref="ArgumentOutOfRangeException">"The number of rows must be greater than 0"</exception>
    /// <exception cref="ArgumentOutOfRangeException">The number of columns must be greater than 0</exception> 
    public Grid2d<T> Resize(int rowCount, int columnCount, T value = default!)
    {
        SizeCheck(rowCount, columnCount);

        _elements.Resize(rowCount);

        foreach (var columns in _elements)
        {
            columns.Resize(columnCount, _clampFunc != null ? _clampFunc(value) : value);
        }

        return this;
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
    /// <exception cref="ArgumentOutOfRangeException">Row index <paramref name="i"/> is out of bound</exception>
    /// <exception cref="ArgumentOutOfRangeException">Column index <paramref name="j"/> is out of bound</exception>
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
    /// <exception cref="ArgumentOutOfRangeException">Row index <paramref name="i"/> is out of bound</exception>
    /// <exception cref="ArgumentOutOfRangeException">Column index <paramref name="j"/> is out of bound</exception>
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
    /// <returns>the current <see cref="Grid2d{T}"/></returns>
    /// <exception cref="ArgumentOutOfRangeException">Row index <paramref name="i"/> is out of bound</exception>
    /// <exception cref="ArgumentOutOfRangeException">Column index <paramref name="j"/> is out of bound</exception>
    public Grid2d<T> SetAt(int i, int j, T value)
    {
        var clampedValue = _clampFunc != null ? _clampFunc(value) : value;
        
        Accessor(i, j, _ => _elements[i][j] = clampedValue);

        return this;
    }

    /// <summary>
    /// Checks that the specified coordinates are within the bounds of the grid
    /// </summary>
    /// <param name="i">the row index of the element to check</param>
    /// <param name="j">the column index of the element to check</param>
    /// <returns>true if the coordinates are within the bounds of the <see cref="Grid2d{T}"/>; false otherwise</returns>
    public bool WithinBound(int i, int j)
    {
        return BoundaryCheck(i, j, false);
    }

    /// <summary>
    /// Returns a sub-<see cref="Grid2d{T}"/> of the current <see cref="Grid2d{T}"/>
    /// of the specified size at the specified coordinates.
    /// </summary>
    /// <param name="i">the row index of the element at which the sub-grid starts</param>
    /// <param name="j">the column index of the element at which the sub-grid starts</param>
    /// <param name="rowCount">the number of rows of the sub-grid</param>
    /// <param name="columnCount">the number of columns of the sub-grid</param>
    /// <returns>the resulting sub-grid</returns>
    /// <exception cref="ArgumentOutOfRangeException">Row index <paramref name="i"/> is out of bound</exception>
    /// <exception cref="ArgumentOutOfRangeException">Column index <paramref name="j"/> is out of bound</exception>
    /// <exception cref="ArgumentOutOfRangeException">"The number of rows must be greater than 0"</exception>
    /// <exception cref="ArgumentOutOfRangeException">The number of columns must be greater than 0</exception>
    public Grid2d<T> SubGrid(int i, int j, int rowCount, int columnCount)
    {
        SizeCheck(rowCount, columnCount);
        BoundaryCheck(i, j);
        // clamp the size of the sub-grid within the boundaries of the current grid
        var subGrid = new Grid2d<T>(Math.Min(rowCount, Rows - i), Math.Min(columnCount, Columns - j));
        // select only the elements within the boundaries of the subgrid
        foreach (var tuple in Iterate().Where(t => t.i >= i && t.i < i + rowCount && t.j >= j && t.j < j + columnCount))
        {
            subGrid[tuple.i - i, tuple.j - j] = tuple.element;
        }

        return subGrid;
    }

    /// <summary>
    /// Performs a boundary check and reports which index is out of bound, optionally throw an exception
    /// </summary>
    /// <param name="i">the row index of the element to check</param>
    /// <param name="j">the column index of the element to check</param>
    /// <param name="throwOnError">flag specifying if an exception should be thrown if any index is out of bound</param>
    /// <exception cref="ArgumentOutOfRangeException">Row index <paramref name="i"/> is out of bound</exception>
    /// <exception cref="ArgumentOutOfRangeException">Column index <paramref name="j"/> is out of bound</exception>
    private bool BoundaryCheck(int i, int j, bool throwOnError = true)
    {
        var columnError = j < 0 || j >= Columns;
        var rowError = i < 0 || i >= Rows;

        if (throwOnError)
        {
            if (rowError)
                throw new ArgumentOutOfRangeException(nameof(i), i, "Row index i is out of bound");

            if (columnError)
                throw new ArgumentOutOfRangeException(nameof(j), j, "Column index j is out of bound");
        }

        return !rowError && !columnError;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rowCount">the new number of rows in the <see cref="Grid2d{T}"/> (must be > 1)</param>
    /// <param name="columnCount">the new number of columns in the <see cref="Grid2d{T}"/> (must be > 1)</param>
    /// <exception cref="ArgumentOutOfRangeException">"The number of rows must be greater than 0"</exception>
    /// <exception cref="ArgumentOutOfRangeException">The number of columns must be greater than 0</exception>
    private static void SizeCheck(int rowCount, int columnCount)
    {
        if (rowCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(rowCount), rowCount, "The number of rows must be greater than 0");
        if (columnCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(columnCount), columnCount, "The number of columns must be greater than 0");
    }

    /// <summary>
    /// Internal accessor to get or set the value of the element at the specified row and column index.
    /// </summary>
    /// <param name="i">the row index of the value to set</param>
    /// <param name="j">the column index of the value to set</param>
    /// <param name="accessor">functor used to get or set the value of the element</param>
    /// <returns>the value of the element</returns>
    /// <exception cref="ArgumentOutOfRangeException">Row index <paramref name="i"/> is out of bound</exception>
    /// <exception cref="ArgumentOutOfRangeException">Column index <paramref name="j"/> is out of bound</exception>
    private T Accessor(int i, int j, Func<T, T> accessor)
    {
        BoundaryCheck(i, j);

        return accessor(_elements[i][j]);
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
            for (var j = 0; j < _elements[i].Count; ++j)
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
    
    #region Neighbours

    /// <summary>
    /// Retrieves a dictionary of neighbours for the element at the specified coordinates; optionally using a
    /// predicate to select valid neighbours, as well as a list of indexes to iterate through.
    /// </summary>
    /// <param name="i">the row index of the element for which to look for neighbours</param>
    /// <param name="j">the column index of the element for which to look for neighbours</param>
    /// <param name="predicate">an optional predicate to select valid neighbours</param>
    /// <param name="indexes">a list of indexes to iterate through to look</param>
    /// <returns>a dictionary of neighbours keyed by their coordinates</returns>
    /// <exception cref="ArgumentOutOfRangeException">Row index <paramref name="i"/> is out of bound</exception>
    /// <exception cref="ArgumentOutOfRangeException">Column index <paramref name="j"/> is out of bound</exception>
    public IDictionary<(int i, int j), T> NeighboursAt(int i, int j, Func<T,T,bool>? predicate = null, 
                                                       IList<(int i, int j)>? indexes = null)
    {
        var result = new Dictionary<(int i, int j), T>();
        var current = GetAt(i, j);

        indexes ??= Neighbours;

        foreach (var coords in indexes)
        {
            var row = i + coords.i;
            var col = j + coords.j;
            
            // validate that the coordinates are within bounds
            if (WithinBound(row, col))
            {
                var neighbour = GetAt(row, col);
                // apply the predicate if available
                if (predicate == null || predicate(current, neighbour))
                {
                    result.Add((row, col), neighbour);
                }
            }
        }

        return result;
    }
    
    #endregion

    #region Dictionaries

    /// <summary>
    /// Applies the value contained in the specified dictionary to the <see cref="Grid2d{T}"/>.
    /// </summary>
    /// <param name="values">a dictionary of values keyed by coordinates</param>
    /// <param name="modifiedCount">receives the number of modified values</param>
    /// <returns>the current <see cref="Grid2d{T}"/></returns>
    public Grid2d<T> Apply(IDictionary<(int i, int j), T> values, out int modifiedCount)
    {
        modifiedCount = 0;
        
        foreach (var keyValue in values)
        {
            if (WithinBound(keyValue.Key.i, keyValue.Key.j))
            {
                this[keyValue.Key.i, keyValue.Key.j] = keyValue.Value;
                ++modifiedCount;
            }
        }

        return this;
    }
    
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
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Grid2d<T> other && Equals(other);
    }

    /// <inheritdoc cref="object"/>
    public override int GetHashCode()
    {
        return _elements.GetHashCode();
    }

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
    /// <returns>a string representation of the <see cref="Grid2d{T}"/></returns>
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

    #region IEquatable implementation / equality operators
    
    /// <inheritdoc cref="IEquatable{T}"/>
    public bool Equals(Grid2d<T>? other)
    {
        if (ReferenceEquals(null, other)) 
            return false;
        if (ReferenceEquals(this, other)) 
            return true;
        // check if the grids are the same dimensions
        if (Rows != other.Rows || Columns != other.Columns)
            return false;
        // the grids are equal if they are of the same dimensions and all their values are equal
        return !_elements.Where((t, i) => t.SequenceEqual(other._elements[i]) == false).Any();
    }

    /// <summary>
    /// Equality operator for <see cref="Grid2d{T}"/> objects.
    /// Two grids are equal if they are of the same dimensions and all their values are equal.
    /// </summary>
    /// <param name="left">the first <see cref="Grid2d{T}"/> to compare</param>
    /// <param name="right">the second <see cref="Grid2d{T}"/> to compare</param>
    /// <returns>true if the grids are equal; false otherwise</returns>
    public static bool operator ==(Grid2d<T>? left, Grid2d<T>? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Inequality operator for <see cref="Grid2d{T}"/> objects.
    /// Two grids are different if they are not of the same dimensions or any of their values are not equal.
    /// </summary>
    /// <param name="left">the first <see cref="Grid2d{T}"/> to compare</param>
    /// <param name="right">the second <see cref="Grid2d{T}"/> to compare</param>
    /// <returns>true if the grids are different; false otherwise</returns>
    public static bool operator !=(Grid2d<T>? left, Grid2d<T>? right)
    {
        return !Equals(left, right);
    }

    #endregion
}