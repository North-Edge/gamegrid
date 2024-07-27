namespace NorthEdge.GameGrid.Tests;

/// <summary>
/// A set of functors used as datapoint for the theory
/// </summary>
/// <typeparam name="T">tested type in each fixture</typeparam>
public class TestFunctors<T>
{
    /// <summary>
    /// A functor that returns a random value for the current type
    /// </summary>
    public required Func<int,int,T> Random { get; init; }
    /// <summary>
    /// A functor that returns a fixed value for the current type
    /// </summary>
    public required Func<int,T> Value { get; init; }
    /// <summary>
    /// A functor that clamps a value for the current type
    /// </summary>
    public required Func<T,T> Clamp { get; init; }
    /// <summary>
    /// A functor that checks if the value is above the minimum for the current type
    /// </summary>
    public required Func<T, bool> AboveMin { get; init; }
    /// <summary>
    /// A functor that checks if the value is below the maximum for the current type
    /// </summary>
    public required Func<T, bool> BelowMax { get; init; }
    /// <summary>
    /// A functor that returns the string representation of the current type
    /// </summary>
    public required Func<T, string> String { get; init; }
}