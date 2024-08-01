using System.Collections;
using System.Text;

namespace NorthEdge.GameGrid;

/// <summary>
/// A 2D grid of generic elements
/// </summary>
/// <typeparam name="T">the type of the elements in the <see cref="Grid2d{T}"/></typeparam>
public class Grid2d<T>(): IEnumerable<T>, IEquatable<Grid2d<T>>
{
    #region Properties

    /// <summary>
    /// Arguments passed to events invoked when elements are added or removed.
    /// </summary>
    /// <param name="i">the row index of the element tied to the event</param>
    /// <param name="j">the column index of the element tied to the event</param>
    /// <param name="element">the element tied to the event</param>
    public sealed record ElementEventArgs(int i, int j, T element);
    /// <summary>
    /// The elements of the <see cref="Grid2d{T}"/> as list of columns containing a list of rows
    /// </summary>
    private readonly List<List<T>> _elements = [];
    /// <summary>
    /// A functor used to clamp the value of the elements of the <see cref="Grid2d{T}"/> when they are modified
    /// </summary>
    private Func<T, T>? _clampFunc;
    /// <summary>
    /// An event invoked when an element is removed from the grid
    /// </summary>
    private EventHandler<ElementEventArgs>? _onElementRemovedEvent { get; }
    /// <summary>
    /// An event invoked when an element is added to the grid
    /// </summary>
    private EventHandler<ElementEventArgs>? _onElementAddedEvent { get; }
    /// <summary>
    /// Stack of events raised by the grid.
    /// </summary>
    private readonly Stack<(int i, int j, bool added)> _eventStack = new();

    /// <summary>
    /// The number of rows in the <see cref="Grid2d{T}"/>
    /// </summary>
    public int Rows { get; private set; }
    /// <summary>
    /// The number of columns in the <see cref="Grid2d{T}"/>
    /// </summary>
    public int Columns { get; private set; }

    #endregion

    #region Constructor
    
    /// <summary>
    /// The <see cref="Grid2d{T}"/> constructor
    /// </summary>
    /// <param name="rows">the number of rows in the <see cref="Grid2d{T}"/> (must be > 1)</param>
    /// <param name="columns">the number of columns in the <see cref="Grid2d{T}"/> (must be > 1)</param>
    /// <param name="onElementAddedEvent">an event invoked when an element is added to the grid</param>
    /// <param name="onElementRemovedEvent">an event invoked when an element is removed from the grid</param>
    /// <param name="value">the default value for the elements of the <see cref="Grid2d{T}"/></param>
    /// <param name="clampFunc">the optional functor used to clamp the value of the elements of the <see cref="Grid2d{T}"/></param>
    public Grid2d(int rows, int columns, T value = default!, Func<T,T>? clampFunc = null, 
                  EventHandler<ElementEventArgs>? onElementAddedEvent = null,
                  EventHandler<ElementEventArgs>? onElementRemovedEvent = null): this()
    {
        _onElementRemovedEvent = onElementRemovedEvent;
        _onElementAddedEvent = onElementAddedEvent;
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

        return InternalResize(rowCount, columnCount, value);
    }

    /// <summary>
    /// Empties the grid of all its elements.
    /// </summary>
    /// <returns>the current <see cref="Grid2d{T}"/></returns>
    public Grid2d<T> Clear()
    {
        return InternalResize(0, 0);
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
    private Grid2d<T> InternalResize(int rowCount, int columnCount, T value = default!)
    {
        if (rowCount == Rows && columnCount == Columns)
            return this;
        
        var colStart = Math.Max(Columns, columnCount);
        var rowStart = Math.Max(Rows, rowCount);
        var colDiff = Columns - columnCount;
        var rowDiff = Rows - rowCount;
        var oldCol = Columns;
        var oldRow = Rows;

        Columns = columnCount;
        Rows = rowCount;

        if (rowDiff < 0)
        {
            // preallocate enough items in the list
            _elements.Capacity = rowCount;

            for (var i = oldRow; i < rowCount; ++i)
            {
                _elements.Insert(i, new List<T>(columnCount));
            }
        }

        for (var i = rowStart; --i >= 0;)
        {
            for (var j = colStart; --j >= 0;)
            {
                // if shrinking rows, delete the elements starting at 0, up to the old column count 
                // if shrinking columns, delete the elements from the new row count to the old count   
                if ((rowDiff > 0 && i >= oldRow) || (colDiff > 0 && j >= columnCount))
                {
                    var element = _elements[i][j];
                    // remove the element from the list
                    _elements[i].RemoveAt(j);
                    // trigger the event to notify that the element was removed
                    OnElementChanged(i, j, element, false);
                }
                // if growing rows, create a whole row starting from 0 to the desired size
                // if growing columns, create new elements from the old size to the new one
                else if (rowDiff < 0 && i >= oldRow || (colDiff < 0 && j >= oldCol))
                {
                    var element = value ?? CreateElement(value);
                    var col = colStart - j - 1;
                    // insert a new element in the list
                    _elements[i].Add(_clampFunc != null ? _clampFunc(element) : element);
                    // trigger an event to notify that the element was added
                    OnElementChanged(i, col, _elements[i][col], true);
                }
            }
        }
        // remove the lists that are deleted
        if (rowDiff > 0)
        {
            _elements.RemoveRange(rowCount, oldRow - rowCount);
        }

        return this;
    }

    /// <summary>
    /// Creates a new element of type T
    /// </summary>
    /// <param name="defaultValue">a default value in case the type doesn't have a parameterless constructor</param>
    /// <returns>a new element of type T</returns>
    private static T CreateElement(T defaultValue)
    {
        return typeof(T).GetConstructor(Type.EmptyTypes) == null 
             ? defaultValue : Activator.CreateInstance<T>();
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
        return this.Select(clampFunc);
    }

    /// <summary>
    /// Clamps the value of the elements of the <see cref="Grid2d{T}"/> using the specified clamping functor.
    /// </summary>
    /// <param name="clampFunc">the clamping functor used to clamp the values</param>
    /// <returns>the current <see cref="Grid2d{T}"/></returns>
    public Grid2d<T> ClampValues(Func<T,T> clampFunc)
    {
        return Update((i, j) => clampFunc(this[i, j]));
    }

    #endregion
    
    #region Events
    
    /// <summary>
    /// Raises an event if an element is added or removed.
    /// </summary>
    /// <param name="i">the row index of the element tied to the event</param>
    /// <param name="j">the column index of the element tied to the event</param>
    /// <param name="element">the element tied to the event</param>
    /// <param name="added">flag specifying if the element has been added or removed</param>
    /// <remarks>Keeps track of the current event with a stack to prevent infinite loops.</remarks>
    private void OnElementChanged(int i, int j, T element, bool added)
    {
        // check if the element in current event is tied to the same element as the previous event 
        if (_eventStack.TryPeek(out var previous) == false || previous.added != added || (previous.i != i && previous.j != j))
        {
            var invokedEvent = added ? _onElementAddedEvent : _onElementRemovedEvent;

            if (invokedEvent != null)
            {
                _eventStack.Push((i, j, added));
                invokedEvent.Invoke(this, new ElementEventArgs(i, j, element));
                _eventStack.Pop();
            }
        }
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
    private T GetAt(int i, int j)
    {
        BoundaryCheck(i, j);

        return _elements[i][j];
    }

    /// <summary>
    /// Sets the value of the element at the specified row and column index
    /// </summary>
    /// <param name="i">the row index of the value to set</param>
    /// <param name="j">the column index of the value to set</param>
    /// <param name="value">the new value of the element</param>
    /// <exception cref="ArgumentOutOfRangeException">Row index <paramref name="i"/> is out of bound</exception>
    /// <exception cref="ArgumentOutOfRangeException">Column index <paramref name="j"/> is out of bound</exception>
    private void SetAt(int i, int j, T value)
    {
        BoundaryCheck(i, j);

        var clampedValue = _clampFunc != null ? _clampFunc(value) : value;
        var element = _elements[i][j];

        _elements[i][j] = clampedValue;
        OnElementChanged(i, j, element, false);
        OnElementChanged(i, j, _elements[i][j], true);
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
        foreach (var indexes in Iterate().Where(t => t.i >= i && t.i < i + rowCount && t.j >= j && t.j < j + columnCount))
        {
            subGrid[indexes.i - i, indexes.j - j] = indexes.element;
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
    /// Checks if the specified dimensions are valid.
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
    
    #endregion
    
    #region IEnumerable implementation

    /// <inheritdoc cref="IEnumerable{T}"/>
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return Enumerator();
    }
    
    /// <inheritdoc cref="IEnumerable{T}"/>
    public IEnumerator GetEnumerator()
    {
        return Enumerator();
    }

    /// <summary>Returns an enumerator that iterates through a collection.</summary>
    /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
    private IEnumerator<T> Enumerator()
    {
        foreach (var rows in _elements)
        {
            foreach (var item in rows)
            {
                yield return item;
            }
        }
    }

    #endregion
    
    #region Iterators

    /// <summary>
    /// Retrieves the elements of the row at the specified index.
    /// </summary>
    /// <param name="i">the index of the row</param>
    /// <returns>the row at the specified index</returns>
    /// <exception cref="ArgumentOutOfRangeException">Row index <paramref name="i"/> is out of bound</exception>
    public IEnumerable<T> Row(int i)
    {
        BoundaryCheck(i, 0);
        
        return _elements[i];
    }

    /// <summary>
    /// Retrieves the elements of the column at the specified index.
    /// </summary>
    /// <param name="j">the index of the column</param>
    /// <returns>the column at the specified index</returns>
    /// <exception cref="ArgumentOutOfRangeException">Column index <paramref name="j"/> is out of bound</exception>
    public IEnumerable<T> Column(int j)
    {
        BoundaryCheck(0, j);

        return Iterate().Where(indexes => indexes.j == j)
                        .Select(indexes => indexes.element);
    }
    
    /// <summary>
    /// Traverses all the elements of the <see cref="Grid2d{T}"/> and update the values
    /// using the specified action that takes the row and column indexes as its parameters. 
    /// </summary>
    /// <param name="transformFunc">the action to apply on all the elements</param>
    /// <returns>the current <see cref="Grid2d{T}"/></returns>
    public Grid2d<T> Update(Func<int, int, T> transformFunc)
    {
        foreach (var index in Iterate())
        {
            this[index.i, index.j] = transformFunc(index.i, index.j);
        }

        return this;
    }
    
    /// <summary>
    /// Traverses all the elements of the <see cref="Grid2d{T}"/> and update the values using the specified action. 
    /// </summary>
    /// <param name="transformFunc">the action to apply on all the elements</param>
    /// <returns>the current <see cref="Grid2d{T}"/></returns>
    public Grid2d<T> Update(Func<T> transformFunc)
    {
        foreach (var indexes in Iterate())
        {
            this[indexes.i, indexes.j] = transformFunc();
        }

        return this;
    }

    /// <summary>
    /// Iterates on all the elements of the <see cref="Grid2d{T}"/> and apply the specified transform functor receiving
    /// the row, column and value as its parameters; returning temporary enumerable of the resulting elements.
    /// </summary>
    /// <returns>enumerable of the transformed values</returns>
    public IEnumerable<TResult> Traverse<TResult>(Func<int, int, T, TResult> transformFunc)
    {
        return Iterate().Select(indexes => transformFunc(indexes.i, indexes.j, indexes.element));
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

        indexes ??= [
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
        return Traverse((i, j, element) => new KeyValuePair<(int i, int j), T>((i, j), element)).ToDictionary();
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

        foreach(var indexes in Iterate())
        {
            var value = (indexes.i, indexes.j);

            if (result.TryGetValue(indexes.element, out var list))
            {
                list.Add(value);
            }
            else
            {
                result[indexes.element] = [value];
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
        var rw = Rows.ToString().Length + 2; // +2 => []
        var cw = Columns.ToString().Length;
        var stringBuilder = new StringBuilder();
        // determine the width of the longest element +2 => []
        var maxWidth = this.Aggregate(cw + 2, (max, element) =>
        {
            var elementStr = transformFunc(element)?.ToString() ?? "null";

            return Math.Max(max, elementStr.Length);
        });
        // add the padding for the height before printing the header
        stringBuilder.Append(new string(' ', rw));
        // print the header consisting of all the column indexes
        for (var j = 0; j < Columns; ++j)
            stringBuilder.Append($"|{j}|".PadLeft(maxWidth + margin));
        // print all the element of the grid
        stringBuilder = Iterate().Aggregate(stringBuilder, (sb, indexes) => {
            var elementStr = transformFunc(indexes.element)?.ToString() ?? "null";
            // print row index at the beginning of each row
            if (indexes.j == 0)
                sb.Append('\n' + $"[{indexes.i}]".PadLeft(rw));
            // print the element
            sb.Append(elementStr.PadLeft(maxWidth + margin));

            return sb;
        });

        return stringBuilder.ToString();
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