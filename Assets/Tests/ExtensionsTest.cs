using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ExtensionsTest
{
    [Test]
    /// <summary>
    /// param T exceptFor mutable arguments of ForEach() can filter non nullable objects.
    /// </summary>
    public void _001_ForEachExceptForWithNonNullableTest()
    {
        var positions = new Pos[] {
            new Pos(0, 1), new Pos(1, 1), new Pos(3, 4), new Pos(4, 5),
            new Pos(0, 1), new Pos(1, 1), new Pos(3, 4), new Pos(4, 5),
            new Pos(1, 2), new Pos(2, 2), new Pos(5, 6), new Pos(6, 7),
            new Pos(4, 1), new Pos(4, 1), new Pos(3, 4), new Pos(6, 7),
        };

        positions.ForEach(pos =>
        {
            Assert.AreNotEqual(new Pos(3, 4), pos, "pos(" + pos.x + ", " + pos.y + "), compared with (3, 4)");
            Assert.AreNotEqual(new Pos(4, 5), pos, "pos(" + pos.x + ", " + pos.y + "), compared with (4, 5)");
        }, new Pos(4, 5), new Pos(3, 4));
    }

    [Test]
    /// <summary>
    /// IEnumerable<Pos> filters argument of ForEach() can filter non nullable objects.
    /// </summary>
    public void _002_ForEachFilterWithNonNullableTest()
    {
        var positions = new Pos[] {
            new Pos(0, 1), new Pos(1, 1), new Pos(3, 4), new Pos(4, 5),
            new Pos(0, 1), new Pos(1, 1), new Pos(3, 4), new Pos(4, 5),
            new Pos(1, 2), new Pos(2, 2), new Pos(5, 6), new Pos(6, 7),
            new Pos(4, 1), new Pos(4, 1), new Pos(3, 4), new Pos(6, 7),
        };

        var filters = new List<Pos>();
        filters.Add(new Pos(0, 1));
        filters.Add(new Pos(6, 7));

        positions.ForEach(pos =>
        {
            Assert.AreNotEqual(new Pos(0, 1), pos, "pos(" + pos.x + ", " + pos.y + "), compared with (0, 1)");
            Assert.AreNotEqual(new Pos(6, 7), pos, "pos(" + pos.x + ", " + pos.y + "), compared with (6, 7)");
        }, filters);
    }

    [Test]
    /// <summary>
    /// param T exceptFor mutable arguments of ForEach() can filter non nullable objects.
    /// </summary>
    public void _003_ForEachFilterWithNullableTest()
    {
        var dirs = new IDirection[] { Direction.north, new North(), new West(), null, Direction.east, new West() };

        dirs.ForEach(dir =>
        {
            Assert.IsNotNull(dir);
            Assert.AreNotEqual(Direction.east, dir);
            Assert.AreNotEqual(Direction.north, dir);
        }, null, Direction.north, Direction.east);
    }

    [Test]
    /// <summary>
    /// param T exceptFor mutable arguments of ForEach() can filter null.
    /// </summary>
    public void _004_ForEachFilterWithNullTest()
    {
        var dirs = new IDirection[] { Direction.north, new North(), new West(), null, Direction.east, new West() };

        dirs.ForEach(dir =>
        {
            Assert.IsNotNull(dir);
        }, null);
    }
}
