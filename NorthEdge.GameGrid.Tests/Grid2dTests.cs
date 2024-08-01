namespace NorthEdge.GameGrid.Tests;

/// <summary>
/// Tests the <see cref="Grid2d{T}"/> using different types for the element type
/// </summary>
/// <typeparam name="T">tested type in each fixture</typeparam>
[TestFixture(typeof(ValuesEnum))]
[TestFixture(typeof(TestValue))]
[TestFixture(typeof(int))]
public class Grid2dTests<T>
{
    /// <summary>
    /// A value out of the default bounds <seealso cref="Grid2dTests{T}.Bound"/>
    /// </summary>
    private const int OutOfBoundDefault = 99;
    /// <summary>
    /// The default bound for random values in the test
    /// </summary>
    private const int Bound = 10;

    /// <summary>
    /// Datapoint for the <see cref="ValuesEnum"/> type
    /// </summary>
    [Datapoint] public TestFunctors<ValuesEnum> enumFunctor = new()
    {
        Clamp = element => element < 0 ? ValuesEnum.Red : element > ValuesEnum.Magenta ? ValuesEnum.Magenta : element,
        Random = (min,max) => (ValuesEnum)new Random(Guid.NewGuid().GetHashCode()).Next(min, max),
        BelowMax = element => element <= ValuesEnum.Magenta,
        String = value => $"     |0|\n[0] {value}",
        AboveMin = element => element >= ValuesEnum.Red,
        Value = value => (ValuesEnum)value
    };
    /// <summary>
    /// Datapoint for the <see cref="int"/> type
    /// </summary>
    [Datapoint] public TestFunctors<int> intFunctor = new()
    {
        Random = (min,max) => new Random(Guid.NewGuid().GetHashCode()).Next(min, max),
        Clamp = element => Math.Min(Math.Max(element, -5), 5),
        String = value => $"    |0|\n[0]   {value}",
        AboveMin = element => element >= -5,
        BelowMax = element => element <= 5,
        Value = value => value
    };
    /// <summary>
    /// Datapoint for the <see cref="TestValue"/> type
    /// </summary>
    [Datapoint] public TestFunctors<TestValue> valueFunctor = new()
    {
        Random = (min,max) => new TestValue((ValuesEnum)new Random(Guid.NewGuid().GetHashCode()).Next(min, max)),
        BelowMax = element => element.Value <= ValuesEnum.Magenta,
        Value = value => new TestValue((ValuesEnum)value),
        AboveMin = element => element.Value >= ValuesEnum.Red,
        String = value => $"     |0|\n[0] {value}",
        Clamp = value => value.Clamped()
    };

    /// <summary>
    /// Tests the <see cref="Grid2d{T}"/> constructor with a size and default value for each type in the fixture
    /// </summary>
    /// <param name="functor">the datapoint for the current theory</param>
    [Theory]
    public void TestWithConstructorWithValue(TestFunctors<T> functor)
    {
        // create a grid from a default value
        var value = functor.Value(OutOfBoundDefault);
        var grid = new Grid2d<T>(5, 5, value);
        // all the elements should have the same initial value
        Assert.That(grid.All(i => i != null && i.Equals(value)));
    }

    /// <summary>
    /// Tests the <see cref="Grid2d{T}"/> constructor with a size,
    /// a random value and clamping for each type in the fixture
    /// </summary>
    /// <param name="functor">the datapoint for the current theory</param>
    [Theory]
    public void TestConstructorWithClampedValue(TestFunctors<T> functor)
    {
        // create a grid from a random value
        var value = functor.Value(OutOfBoundDefault);
        var grid = new Grid2d<T>(1, 1, value, functor.Clamp);
        // all the values should have been clamped upon initialization
        Assert.That(grid.All(i => i != null && functor.BelowMax(i) 
                                            && functor.AboveMin(i)));
    }

    /// <summary>
    /// Tests the <see cref="Grid2d{T}"/> constructor with a size,
    /// a valid value and clamping for each type in the fixture
    /// </summary>
    /// <param name="functor">the datapoint for the current theory</param>
    [Theory]
    public void TestConstructorWithDefaultClampedValue(TestFunctors<T> functor)
    {
        // create a grid from a default value
        var value = functor.Value(2);
        var grid = new Grid2d<T>(1, 1, value, functor.Clamp);
        // all the values should be valid and unchanged upon initialization
        Assert.That(grid.All(i => i != null && i.Equals(value)
                                            && functor.BelowMax(i) 
                                            && functor.AboveMin(i)));
    }

    /// <summary>
    /// Tests updating the elements of the <see cref="Grid2d{T}"/> 
    /// </summary>
    /// <param name="functor">the datapoint for the current theory</param>
    [Theory]
    public void TestUpdateValues(TestFunctors<T> functor)
    {
        // initialize the grid with a value that cannot be picked randomly
        var value = functor.Value(OutOfBoundDefault);
        var grid = new Grid2d<T>(5, 5, value);
        // update all the values with a random value within the bounds
        grid.Update(() => functor.Random(-Bound, Bound));
        // all the values should be equal different from the out of bound default
        foreach (var element in grid)
        {
            Assert.That(element, Is.Not.EqualTo(value));
        }
    }

    /// <summary>
    /// Tests updating the elements of the <see cref="Grid2d{T}"/> with clamping
    /// </summary>
    /// <param name="functor">the datapoint for the current theory</param>
    [Theory]
    public void TestUpdateWithClampedValues(TestFunctors<T> functor)
    {
        // initialize the grid with a valid value
        var value = functor.Value(-1);
        var grid = new Grid2d<T>(5, 5, value, functor.Clamp);
        // update all the values with a random value within the bounds
        grid.Update(() => functor.Random(0, Bound));
        // all the values should have been updated and clamped 
        Assert.That(grid.All(i => i != null && !i.Equals(value)
                                            && functor.BelowMax(i)
                                            && functor.AboveMin(i)));
    }

    /// <summary>
    /// Tests evaluating a temporary collection of elements with clamped values without modifying the grid
    /// </summary>
    /// <param name="functor">the datapoint for the current theory</param>
    [Theory]
    public void TestTemporaryClampedValues(TestFunctors<T> functor)
    {
        // initialize the grid with a value that cannot be picked randomly
        var value = functor.Value(OutOfBoundDefault);
        var grid = new Grid2d<T>(5, 5, value);
        // the temporary clamped values should be within bounds 
        Assert.That(grid.ClampedValues(functor.Clamp).All(i => i != null && functor.BelowMax(i) && functor.AboveMin(i)));
        // the values shouldn't have been updated
        foreach (var element in grid)
        {
            Assert.That(element, Is.EqualTo(value));
        }
    }
    
    /// <summary>
    /// Tests updating the grid with clamped values
    /// </summary>
    /// <param name="functor">the datapoint for the current theory</param>
    [Theory]
    public void TestClampingValues(TestFunctors<T> functor)
    {
        var grid = new Grid2d<T>(5, 5);
        // update all the values with a random value
        grid.Update(() => functor.Random(-Bound, Bound));
        // update the grid with clamped values
        grid.ClampValues(functor.Clamp);
        // all the values should have been clamped
        Assert.That(grid.All(i => i != null && functor.BelowMax(i) && functor.AboveMin(i)));
        // update all the values with a value out of the clamping bounds
        var value = functor.Value(OutOfBoundDefault);
        grid.Update(() => value);
        // all the values should now be equal to the out-of-bound value
        Assert.That(grid.All(i => i != null && i.Equals(value)));
    }

    /// <summary>
    /// Tests converting the <see cref="Grid2d{T}"/> to string
    /// </summary>
    /// <param name="functor">the datapoint for the current theory</param>
    [Theory]
    public void TestToString(TestFunctors<T> functor)
    {
        // update all the values with a default value
        var value = functor.Value(1);
        var expected = functor.String(value);
        var grid = new Grid2d<T>(1, 1, value);

        // the resulting string should match the expected output
        Assert.That(grid.ToString(), Is.EqualTo(expected));
    }

    /// <summary>
    /// Tests iterating the <see cref="Grid2d{T}"/> and updating/applying the clamping function
    /// </summary>
    /// <param name="functor">the datapoint for the current theory</param>
    [Theory]
    public void TestIteratorWithClamp(TestFunctors<T> functor)
    {
        var grid = new Grid2d<T>(5, 5);
        var iteration = 0;
        var index = 0;
        // set the values to an increasing value  
        grid.Update(() => functor.Value(++index));
        // set the clamping function but don't apply it yet
        foreach (var element in grid.SetClamp(functor.Clamp, false))
        {
            // the values should still be the same despite setting the clamping function
            Assert.That(element, Is.EqualTo(functor.Value(++iteration)));
        }
        // set the values to random values  
        grid.Update(() => functor.Random(-Bound, Bound));
        // iterate as a dictionary of elements keyed by (row,column) structures
        Assert.Multiple(() => {
            foreach (var keyValue in grid.ToDictionary())
            {
                // each value of the dictionary should match the value in the grid at the coordinates in the key
                Assert.That(keyValue.Value, Is.EqualTo(grid[keyValue.Key.i, keyValue.Key.j]));
                // all the values should have been clamped because the clamping function was set
                Assert.That(functor.AboveMin(keyValue.Value) && functor.BelowMax(keyValue.Value));
            }
        });
        var value = functor.Value(OutOfBoundDefault);
        // remove the clamping function and update all the values with a value out of the clamping bounds
        grid.SetClamp(null, true).Update(() => value);

        Assert.Multiple(() =>
        {
            // all the values should now be equal to the out-of-bound value
            Assert.That(grid.All(i => i != null && i.Equals(value)));
            // set the clamping function again and apply it straight away: all the values should have been clamped
            Assert.That(grid.SetClamp(functor.Clamp, true).All(i => functor.AboveMin(i) && functor.BelowMax(i)));
        });
    }

    /// <summary>
    /// Tests the comparison operators of the <see cref="Grid2d{T}"/>
    /// </summary>
    /// <param name="functor">the datapoint for the current theory</param>
    [Theory]
    public void TestComparisons(TestFunctors<T> functor)
    {
        var grid1 = new Grid2d<T>(5, 5);
        var grid2 = new Grid2d<T>(5, 5);
        var grid3 = new Grid2d<T>(3, 3);
        var index = 0;
        
        // the grids should be equal
        Assert.Multiple(() =>
        {
            Assert.That(grid1.Equals(grid1), Is.True);
            Assert.That(grid1 == grid2, Is.True);
            // set the values to an increasing value  
            grid1.Update(() => functor.Value(++index));
            // the grids shouldn't be equal anymore
            Assert.That(grid1 != grid2, Is.True);
            Assert.That(grid3.Equals(null), Is.False);
            Assert.That(grid3, Is.Not.EqualTo(grid2));
            Assert.That(grid2.Resize(3, 3), Is.EqualTo(grid3));
        });
    }

    /// <summary>
    /// Tests the events when elements are added or removed from the <see cref="Grid2d{T}"/>
    /// </summary>
    /// <param name="functor">the datapoint for the current theory</param>
    [Theory]
    public void TestElementEvents(TestFunctors<T> functor)
    {
        var onElementRemovedCount = 0;
        var onElementAddedCount = 0;
        // create a new grid filled with 0
        var grid = new Grid2d<T>(null, (_, _) => ++onElementAddedCount,
                                       (_, _) => ++onElementRemovedCount);
        // resize the grid then empty it
        var count = grid.Resize(5, 5).Count();
        // keep a list of all the elements of the grid for later
        var elements = grid.ToList();
        // empty the list
        grid.Clear();
        // check that the grid dimensions are now 0
        Assert.That(grid.Columns, Is.EqualTo(0));
        Assert.That(grid.Rows, Is.EqualTo(0));
        // check that the callbacks have been called for each element
        Assert.That(onElementAddedCount, Is.EqualTo(count));
        Assert.That(onElementRemovedCount, Is.EqualTo(count));
        // check that the old elements of the list that are disposable have been disposed by the event handler
        Assert.That(elements.All(element => element is not TestValue testValue
                                         || testValue is { IsDisposed: true, DisposeCalls: 1 }));
    }

    /// <summary>
    /// Tests the comparison operators of the <see cref="Grid2d{T}"/>
    /// </summary>
    /// <param name="functor">the datapoint for the current theory</param>
    [Theory]
    public void TestElementEventsRecursion(TestFunctors<T> functor)
    {
        var onElementRemovedCount = 0;
        var onElementAddedCount = 0;
        // create a new grid filled with 0
        var grid = new Grid2d<T>(5, 5, functor.Value(0), null, 
        (obj, args) => {
            var g = obj as Grid2d<T>;
            // ReSharper disable once AccessToModifiedClosure
            ++onElementAddedCount;
            
            if (g != null)
            {
                // check that replacing the element in the element added event doesn't trigger an infinite loop
                g[args.i, args.j] = functor.Value(5);
            }
        }, 
        (_, args) => {
            // ReSharper disable once AccessToModifiedClosure
            ++onElementRemovedCount;
            // dispose the object if it is disposable
            if (args.element is IDisposable disposable) {
                disposable.Dispose();
            }
        });
        var count = grid.Count();
        Assert.Multiple(() =>
        {
            // check that the both callbacks have been called for each element because of the affectation
            Assert.That(onElementAddedCount, Is.EqualTo(count));
            Assert.That(onElementRemovedCount, Is.EqualTo(count));
        });
        // resize the grid
        onElementAddedCount = onElementRemovedCount = 0;
        var count2 = grid.Resize(10, 10).Count();
        // keep a list of all the elements of the grid for later
        var elements = grid.ToList();
        // assert that all the elements have been set to 5 using the event handler
        Assert.That(elements.All(e => e != null && e.Equals(functor.Value(5))));
        Assert.Multiple(() =>
        {
            // check that the callbacks have been called for each new element because of the affectation
            Assert.That(onElementAddedCount, Is.EqualTo(count2 - count));
            Assert.That(onElementRemovedCount, Is.EqualTo(count2 - count));
        });
        // empty the list
        onElementAddedCount = onElementRemovedCount = 0;
        grid.Clear();
        Assert.Multiple(() =>
        {
            // check that the removed callbacks have been called for each element
            Assert.That(onElementAddedCount, Is.EqualTo(0));
            Assert.That(onElementRemovedCount, Is.EqualTo(count2));
        });
        // check that the old elements of the list that are disposable have been disposed by the event handler
        Assert.That(elements.All(element => element is not TestValue testValue
                                         || testValue is { IsDisposed: true, DisposeCalls: 1 }));
    }
}