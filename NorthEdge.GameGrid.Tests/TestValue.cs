namespace NorthEdge.GameGrid.Tests;

/// <summary>
/// A test object used as a type in the tests using generic types.
/// </summary>
/// <param name="value">the internal value of the test object</param>
public class TestValue(ValuesEnum value = ValuesEnum.Pink): IEquatable<TestValue>, IDisposable
{
    /// <summary>
    /// The internal value of the test object
    /// </summary>
    public readonly ValuesEnum Value = value;
    /// <summary>
    /// Flag specifying if the object is disposed
    /// </summary>
    public bool IsDisposed { get; private set; }
    /// <summary>
    /// Counts the number of calls to Dispose
    /// </summary>
    public int DisposeCalls { get; private set; }

    /// <summary>
    /// Clamps the value of the test object
    /// </summary>
    /// <returns>a new TestValue with the clamped value or this object if no clamping occured</returns>
    public TestValue Clamped()
    {
        return Value switch {
            > ValuesEnum.Magenta => new TestValue(ValuesEnum.Magenta),
            < ValuesEnum.Red => new TestValue(ValuesEnum.Red),
            _ => this
        };
    }

    #region IEquatable implementation

    /// <inheritdoc cref="IEquatable{T}"/>
    public bool Equals(TestValue? other)
    {
        return other != null && Value == other.Value;
    }

    #endregion

    #region object overrides

    /// <inheritdoc cref="object"/>
    public override bool Equals(object? obj)
    {
        return obj != null && obj.GetType() == GetType() && Equals(obj as TestValue);
    }

    /// <inheritdoc cref="object"/>
    public override int GetHashCode()
    {
        return (int)Value;
    }

    /// <inheritdoc cref="object"/>
    public override string ToString()
    {
        return Value.ToString();
    }

    #endregion

    /// <inheritdoc cref="IDisposable"/>
    public void Dispose()
    {
        ++DisposeCalls;

        if (IsDisposed == false)
        {
            GC.SuppressFinalize(this);
            IsDisposed = true;
        }
    }
}