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
        const int columns = 8;
        const int rows = 10;
        var index = 0;

        // expand the grid
        grid.Resize(rows, columns);
        // the size should match the new dimensions
        Assert.Multiple(() =>
        {
            Assert.That(grid.Rows, Is.EqualTo(rows));
            Assert.That(grid.Columns, Is.EqualTo(columns));
            Assert.That(grid.Count(), Is.EqualTo(rows * columns));
        });
        // try to resize the grid to the same size
        grid.Resize(rows, columns);
        // the grid size shouldn't have changed
        Assert.Multiple(() =>
        {
            Assert.That(grid.Rows, Is.EqualTo(rows));
            Assert.That(grid.Columns, Is.EqualTo(columns));
            Assert.That(grid.Count(), Is.EqualTo(rows * columns));
        });
        // expand the grid, only changing the columns
        grid.Resize(rows, columns + 2);
        // the size should match the new dimensions
        Assert.Multiple(() =>
        {
            Assert.That(grid.Rows, Is.EqualTo(rows));
            Assert.That(grid.Columns, Is.EqualTo(columns + 2));
            Assert.That(grid.Count(), Is.EqualTo(rows * (columns + 2)));
        });
        // expand the grid, only changing the rows
        grid.Resize(rows + 5, columns + 2);
        grid.Update(() => ++index);
        // the size should match the new dimensions
        Assert.Multiple(() =>
        {
            Assert.That(grid.Rows, Is.EqualTo(rows + 5));
            Assert.That(grid.Columns, Is.EqualTo(columns + 2));
            Assert.That(grid.Count(), Is.EqualTo((rows + 5) * (columns + 2)));
        });
        // shrink the grid
        grid.Resize(rows - 3, columns - 1);
        // the size should match the new dimensions
        Assert.Multiple(() =>
        {
            Assert.That(grid.Rows, Is.EqualTo(rows - 3));
            Assert.That(grid.Columns, Is.EqualTo(columns - 1));
            Assert.That(grid.Count(), Is.EqualTo((rows - 3) * (columns - 1)));
        });
        // shrink the grid, only changing the columns
        grid.Resize(rows - 3, columns - 3);
        // the size should match the new dimensions
        Assert.Multiple(() =>
        {
            Assert.That(grid.Rows, Is.EqualTo(rows - 3));
            Assert.That(grid.Columns, Is.EqualTo(columns - 3));
            Assert.That(grid.Count(), Is.EqualTo((rows - 3) * (columns - 3)));
        });
        // shrink the grid, only changing the rows
        grid.Resize(rows - 5, columns - 3);
        // the size should match the new dimensions
        Assert.Multiple(() =>
        {
            Assert.That(grid.Rows, Is.EqualTo(rows - 5));
            Assert.That(grid.Columns, Is.EqualTo(columns - 3));
            Assert.That(grid.Count(), Is.EqualTo((rows - 5) * (columns - 3)));
        });
    }
    
    /// <summary>
    /// Test the row accessor
    /// </summary>
    [Test]
    public void TestRowAccessor()
    {
        var grid = new Grid2d<int>(3, 3);
        List<int> expected = [4, 5, 6];
        var index = 0;
        // set the values to an increasing value  
        grid.Update(() => ++index);

        Assert.That(grid.Row(1), Is.EqualTo(expected));
        Assert.Throws<ArgumentOutOfRangeException>(() => Assert.That(grid.Row(5), Is.EqualTo(expected)));
    }
    
    /// <summary>
    /// Test the column accessor
    /// </summary>
    [Test]
    public void TestColumnAccessor()
    {
        var grid = new Grid2d<int>(3, 3);
        List<int> expected = [2, 5, 8];
        var index = 0;
        // set the values to an increasing value  
        grid.Update(() => ++index);

        Assert.That(grid.Column(1), Is.EqualTo(expected));
        Assert.Throws<ArgumentOutOfRangeException>(() => Assert.That(grid.Column(5), Is.EqualTo(expected)));
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
    /// Tests the indexers with an invalid row index
    /// </summary>
    [Test]
    public void TestAIndexersWithInvalidCoordinates()
    {
        var grid = new Grid2d<int>();
        Assert.Multiple(() =>
        {
            // the indexer should throw an exception when trying to pass an out-of-bound value for the row index
            Assert.Throws<ArgumentOutOfRangeException>(() => Assert.That(grid[1,0], Is.EqualTo(0)));
            // the indexer should throw an exception when trying to pass an out-of-bound value for the row index
            Assert.Throws<ArgumentOutOfRangeException>(() => grid[1,0] = 0);
            // the indexer should throw an exception when trying to pass an out-of-bound value for the column index
            Assert.Throws<ArgumentOutOfRangeException>(() => Assert.That(grid[0,1], Is.EqualTo(0)));
            // the indexer should throw an exception when trying to pass an out-of-bound value for the column index
            Assert.Throws<ArgumentOutOfRangeException>(() => grid[0,1] = 0);
        });
    }
    
    /// <summary>
    /// Tests the indexers
    /// </summary>
    [Test]
    public void TestValueAccess()
    {
        const int value = 999;
        var grid = new Grid2d<int>(1, 1);

        // the value should be equal to the default value (0)
        Assert.That(grid[0, 0], Is.EqualTo(0));
        // change the value of the element using the indexer
        grid[0, 0] = value;
        // the value should be changed
        Assert.That(grid[0, 0], Is.EqualTo(value));
    }

    /// <summary>
    /// Test that using a nullable type as the element type throws an exception when trying to organize the elements
    /// as a dictionary containing a list of the coordinates of elements keyed by their corresponding value 
    /// </summary>
    [Test]
    public void TestCoordinatesDictionaryWithInvalidType()
    {
        // the method should throw an exception because a nullable type cannot be used as a dictionary key
        Assert.Throws<InvalidOperationException>(() => _ = new Grid2d<int?>(1, 1).ToCoordinates());
    }
        
    /// <summary>
    /// Test transforming the grind into a dictionary containing a list
    /// of the coordinates of elements keyed by their corresponding value
    /// </summary>
    [Test]
    public void TestCoordinatesDictionary()
    {
        var grid = new Grid2d<TestValue>(5, 5);
        // set all the value of the elements to the value of their column index
        grid.Update((_, j) => new TestValue((ValuesEnum)j));
        // transform the grid into a dictionary of coordinates for each value
        var coordinates = grid.ToCoordinates();
        // iterate the coordinates dictionary
        Assert.Multiple(() => {
            foreach (var keyValue in coordinates)
            {
                // all the values should be equal to their column index
                Assert.That(keyValue.Value.All(k => keyValue.Key.Value == (ValuesEnum)k.j));
            }
        });
    }
    
    /// <summary>
    /// Tests the Traverse method
    /// </summary>
    [Test]
    public void TestTraverse()
    {
        List<ValuesEnum> transformedValues = [ValuesEnum.Blue, ValuesEnum.Green, ValuesEnum.Yellow];
        List<int> expectedValues = [1, 2, 4];
        var grid = new Grid2d<int>(1, 3) {
            [0, 0] = 1, // Blue
            [0, 1] = 2, // Green
            [0, 2] = 4  // Yellow
        };

        // the resulting string should match the expected output
        Assert.That(grid.Traverse((_, _, e) => (ValuesEnum)e), Is.EqualTo(transformedValues));
        // the values in the grid should not have changed
        Assert.That(grid, Is.EqualTo(expectedValues));
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

    /// <summary>
    /// Tests the check that verifies coordinates are within the bounds of the grid 
    /// </summary>
    [Test]
    public void TestCoordinatesWithinBound()
    {
        var grid = new Grid2d<int>(5, 5);

        Assert.Multiple(() =>
        {
            Assert.That(grid.WithinBound(0, 0), Is.True);
            Assert.That(grid.WithinBound(5, 5), Is.False);
        });
    }

    /// <summary>
    /// Tests the hash code method for the grid
    /// </summary>
    [Test]
    public void TestHashCode()
    {
        var random = new Random(Guid.NewGuid().GetHashCode());
        var grids = new List<Grid2d<int>>();
        var gridCount = 100;

        for (var n = 0; n < gridCount; ++n)
        {
            var rows = random.Next(1, 25);
            var columns = random.Next(1, 25);
            var grid = new Grid2d<int>(rows, columns);
            // set the values to random values
            grids.Add(grid.Update(() => random.Next(-9999999, 9999999)));
        }

        Assert.That(grids.ToHashSet(), Has.Count.EqualTo(gridCount));
    }
    
    /// <summary>
    /// Test retrieving sub-grids from a grid 
    /// </summary>
    [Test]
    public void TestSubGrid()
    {
        var grid = new Grid2d<int>(5, 5);
        var index = 0;
        // set the values to an increasing value  
        grid.Update(() => ++index);
        
        Assert.Multiple(() =>
        {
            var subgrid = grid.SubGrid(0, 0, 3, 3);

            Assert.That(subgrid.Rows, Is.EqualTo(3));
            Assert.That(subgrid.Columns, Is.EqualTo(3));
            Assert.That(grid.Resize(3, 3), Is.EqualTo(subgrid));

            // check that asking for a subgrid than is available doesn't exceed the bounds of the grid
            subgrid = grid.SubGrid(2, 2, 3, 3);
            Assert.That(subgrid.Rows, Is.EqualTo(1));
            Assert.That(subgrid.Columns, Is.EqualTo(1));
            // check that trying to retrieve a subgrid out of bounds throws an exception
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = grid.SubGrid(4, 4, 1, 1));
            // check that trying to retrieve a subgrid of invalid dimensions
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = grid.SubGrid(0, 0, 0, 0));
        });
    }

    /// <summary>
    /// Tests the Apply method
    /// </summary>
    [Test]
    public void TestApply()
    {
        var appliedValues = new Dictionary<(int,int), int> {
            { (0, 0), 50 },
            { (0, 1), 99 },
            { (0, 2), 10 },
        };
        List<int> expectedValues = [50, 99, 10];
        var grid = new Grid2d<int>(1, 3);

        // the resulting string should match the expected output
        grid.Apply(appliedValues, out var modified);
        Assert.Multiple(() =>
        {
            Assert.That(modified, Is.EqualTo(3));
            // the values in the grid should not have changed
            Assert.That(grid, Is.EqualTo(expectedValues));
        });
    }

    /// <summary>
    /// Tests the neighbours methods
    /// </summary>
    [Test]
    public void TestNeighbours()
    {
        var index = 0;
        var grid1 = new Grid2d<int>(5, 5);
        var grid2 = new Grid2d<int>(3, 3);
        var grid3 = new Grid2d<int>(2, 2);

        // set the values to an increasing value
        grid1.Update(() => ++index);

        IList<(int i, int j)> indexes = [
            (-1,  0), ( 1,  0),
            ( 0, -1), ( 0,  1)
        ];
        var subgrid1 = grid1.SubGrid(0, 0, 3, 3);
        var subgrid2 = grid1.SubGrid(0, 0, 2, 2);
        var neighbours1 = grid1.NeighboursAt(1, 1);
        var neighbours2 = grid1.NeighboursAt(0, 0);
        var neighbours3 = grid1.NeighboursAt(1, 1, null, indexes);
        var neighbours4 = grid1.NeighboursAt(1, 1, (_, neighbour) => neighbour <= 2);
        var neighbours5 = grid1.NeighboursAt(1, 1, (_, neighbour) => neighbour <= 2, indexes);

        Assert.Multiple(() =>
        {
            Assert.That(neighbours1, Has.Count.EqualTo(8));             // all 8 elements around the target
            Assert.That(neighbours2, Has.Count.EqualTo(3));             // 3 elements because the target is in a corner
            Assert.That(neighbours3, Has.Count.EqualTo(indexes.Count)); // 4 elements matching the indexes
            Assert.That(neighbours4, Has.Count.EqualTo(2));             // 2 elements filtered by the predicate
            Assert.That(neighbours5, Has.Count.EqualTo(1));             // 1 element filtered by the predicate and indexes
            // the only result is the second element of the grid
            Assert.That(neighbours5.First().Key.i, Is.EqualTo(0));
            Assert.That(neighbours5.First().Key.j, Is.EqualTo(1));
            // check that the neighbours match the values in the grid at each of their coordinates
            CheckNeighbours(neighbours1, grid1);
            CheckNeighbours(neighbours2, grid1);
            CheckNeighbours(neighbours3, grid1);
            CheckNeighbours(neighbours4, grid1);
            CheckNeighbours(neighbours5, grid1);
            // the neighbours do not contain the target element so the grids shouldn't be equal
            Assert.That(grid2.Apply(neighbours1, out var modified1), Is.Not.EqualTo(subgrid1));
            Assert.That(modified1, Is.EqualTo(8));
            // now, set the value of the target element in the second grid 
            grid2[1, 1] = subgrid1[1, 1];
            // the grids should now be equal
            Assert.That(grid2, Is.EqualTo(subgrid1));
            // the neighbours do not contain the target element so the grids shouldn't be equal
            Assert.That(grid3.Apply(neighbours2, out var modified2), Is.Not.EqualTo(subgrid2));
            Assert.That(modified2, Is.EqualTo(3));
            // now, set the value of the target element in the second grid 
            grid3[0, 0] = subgrid2[0, 0];
            // the grids should now be equal
            Assert.That(grid3, Is.EqualTo(subgrid2));
        });
    }

    /// <summary>
    /// Verifies that a dictionary of neighbours match the values at their coordinates in the grid
    /// </summary>
    /// <param name="neighbours">the neighbours to check</param>
    /// <param name="grid">the grid to check against</param>
    private static void CheckNeighbours(IDictionary<(int i, int j), int> neighbours, Grid2d<int> grid)
    {
        foreach (var coords in neighbours)
        {
            Assert.That(coords.Value, Is.EqualTo(grid[coords.Key.i, coords.Key.j]));
        }
    }
}