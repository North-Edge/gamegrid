namespace NorthEdge.GameGrid.Tests;

/// <summary>
/// Test suite for the common parts of the <see cref="Grid2d{T}"/> tests
/// </summary>
public class Grid2dCommonTests
{
    /// <summary>
    /// Tests the default constructor of the <see cref="Grid2d{T}"/>
    /// </summary>
    [Test]
    public void TestDefaultConstructor()
    {
        var grid = new Grid2d<int>();
        // the grid should be empty
        Assert.Multiple(() =>
        {
            Assert.That(grid.Rows, Is.EqualTo(0));
            Assert.That(grid.Columns, Is.EqualTo(0));
        });
    }

    /// <summary>
    /// Tests the constructor of the <see cref="Grid2d{T}"/>, specifying the size of the grid
    /// </summary>
    [Test]
    public void TestConstructorWithSize()
    {
        const int rows = 10;
        const int columns = 8;
        var grid = new Grid2d<int>(rows, columns);

        // the size should match the dimensions passed to the constructor
        Assert.Multiple(() =>
        {
            Assert.That(grid.Rows, Is.EqualTo(rows));
            Assert.That(grid.Columns, Is.EqualTo(columns));
        });
    }

    /// <summary>
    /// Tests the resizing of the <see cref="Grid2d{T}"/> (expanding and shrinking)
    /// </summary>
    [Test]
    public void TestResize()
    {
        var grid = new Grid2d<int>();
        const int rows = 10;
        const int columns = 8;

        // expand the grid
        grid.Resize(rows, columns);
        // the size should match the new dimensions
        Assert.Multiple(() =>
        {
            Assert.That(grid.Rows, Is.EqualTo(rows));
            Assert.That(grid.Columns, Is.EqualTo(columns));
        });
        // shrink the grid
        grid.Resize(rows - 3, columns - 1);
        // the size should match the new dimensions
        Assert.Multiple(() =>
        {
            Assert.That(grid.Rows, Is.EqualTo(rows - 3));
            Assert.That(grid.Columns, Is.EqualTo(columns - 1));
        });
    }

    /// <summary>
    /// Test the constructor with an invalid row
    /// </summary>
    [Test]
    public void TestConstructorWithInvalidRow()
    {
        // the constructor should throw an exception
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new Grid2d<int>(0, 1));
    }

    /// <summary>
    /// Test the constructor with an invalid column
    /// </summary>
    [Test]
    public void TestConstructorWithInvalidColumn()
    {
        // the constructor should throw an exception
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new Grid2d<int>(1, 0));
    }

    /// <summary>
    /// Tests the getters/setters with an invalid row index
    /// </summary>
    [Test]
    public void TestAccessWithInvalidRow()
    {
        var grid = new Grid2d<int>(1, 1);
        Assert.Multiple(() =>
        {
            // the getters should throw an exception when trying to pass an out-of-bound value for the row index
            Assert.Throws<ArgumentOutOfRangeException>(() => Assert.That(grid[1,0], Is.EqualTo(0)));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.GetAt(1, 0));
            // the setters should throw an exception when trying to pass an out-of-bound value for the row index
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.SetAt(1, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid[1,0] = 0);
            
        });
    }

    /// <summary>
    /// Tests the getters/setters with an invalid column index
    /// </summary>
    [Test]
    public void TestAccessWithInvalidColumn()
    {
        var grid = new Grid2d<int>(1, 1);
        Assert.Multiple(() =>
        {
            // the getters should throw an exception when trying to pass an out-of-bound value for the column index
            Assert.Throws<ArgumentOutOfRangeException>(() => Assert.That(grid[0,1], Is.EqualTo(0)));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.GetAt(0, 1));
            // the setters should throw an exception when trying to pass an out-of-bound value for the column index
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.SetAt(0, 1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid[0,1] = 0);
        });
    }
    
    /// <summary>
    /// Tests the getters/setters
    /// </summary>
    [Test]
    public void TestValueAccess()
    {
        const int value = 999;
        var grid = new Grid2d<int>(1, 1);
        Assert.Multiple(() =>
        {
            // the value should be equal to the default value (0)
            Assert.That(grid.GetAt(0, 0), Is.EqualTo(0));
            Assert.That(grid[0, 0], Is.EqualTo(0));
            // change the value of the element using the SetAt method and check that the value has changed
            Assert.That(grid.SetAt(0, 0, value).GetAt(0, 0), Is.EqualTo(value));
            // change the value of the element using the indexer
            grid[0, 0] = value + 1;
            // the value should be changed
            Assert.That(grid[0, 0], Is.EqualTo(value + 1));
        });
    }

    /// <summary>
    /// Test that using a nullable type as the element type throws an exception when trying to organize the elements
    /// as a dictionary containing a list of the coordinates of elements keyed by their corresponding value 
    /// </summary>
    [Test]
    public void TestCoordinatesDictionary()
    {
        // the method should throw an exception because a nullable type cannot be used as a dictionary key
        Assert.Throws<InvalidOperationException>(() => _ = new Grid2d<int?>(1, 1).ToCoordinates());
    }
    
    /// <summary>
    /// Tests the cast to string using an action to transform the value of the elements
    /// </summary>
    [Test]
    public void TestToStringWithTransform()
    {
        var expected = $"       |0|    |1|    |2|\n[0]   {ValuesEnum.Blue}  {ValuesEnum.Green} {ValuesEnum.Yellow}";
        var grid = new Grid2d<int>(1, 3) {
            [0, 0] = 1, // Blue
            [0, 1] = 2, // Green
            [0, 2] = 4  // Yellow
        };
        // the resulting string should match the expected output
        Assert.That(grid.ToString(i => (ValuesEnum)i), Is.EqualTo(expected));
    }
}