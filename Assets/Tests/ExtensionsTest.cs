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

    private struct TestBooleanStruct
    {
        public TestBooleanStruct(bool value) { this.value = value; }
        public bool value;
    };

    private class TestBooleanClass
    {
        public TestBooleanClass(bool value) { this.value = value; }
        public bool value;
    };

    [Test]
    /// <summary>
    /// param T exceptFor mutable arguments of ForEach() can filter null. <br />
    /// "pass by value" elements aren't updated by ForEach()
    /// </summary>
    public void _005_ForEachVariableAssignWithBoolCausesUnexpectedBreakTest()
    {
        // setup
        var sutPrimitiveArray = new bool[4].Select(_ => true).ToArray();
        var sutStruct = new TestBooleanStruct[4].Select(_ => new TestBooleanStruct(true)).ToArray();
        var sutClass = new TestBooleanClass[4].Select(_ => new TestBooleanClass(true)).ToArray();
        var sutClassSelect = new TestBooleanClass[4].Select(_ => new TestBooleanClass(true)).ToArray();

        // when
        bool resultPrimitiveArray = sutPrimitiveArray.ForEach(value => value = false);
        bool resultStruct = sutStruct.ForEach(tb => tb.value = false);
        bool resultClass = sutClass.ForEach(tb => tb.value = false);
        sutClassSelect.Select(tb => tb.value = false);

        // then
        Assert.AreEqual(false, resultPrimitiveArray);   // ForEach loop breaks.
        Assert.AreEqual(true, sutPrimitiveArray[0]);    // Primitive type elements aren't updated.
        Assert.AreEqual(true, sutPrimitiveArray[1]);
        Assert.AreEqual(true, sutPrimitiveArray[2]);
        Assert.AreEqual(true, sutPrimitiveArray[3]);

        Assert.AreEqual(false, resultStruct);           // ForEach loop breaks.
        Assert.AreEqual(true, sutStruct[0].value);      // struct elements also aren't updated.
        Assert.AreEqual(true, sutStruct[1].value);
        Assert.AreEqual(true, sutStruct[2].value);
        Assert.AreEqual(true, sutStruct[3].value);

        Assert.AreEqual(false, resultClass);            // ForEach loop breaks.
        Assert.AreEqual(false, sutClass[0].value);      // Only the first element is updated.
        Assert.AreEqual(true, sutClass[1].value);
        Assert.AreEqual(true, sutClass[2].value);
        Assert.AreEqual(true, sutClass[3].value);

        Assert.AreEqual(true, sutClassSelect[0].value);      // No elements are updated by Select.
        Assert.AreEqual(true, sutClassSelect[1].value);
        Assert.AreEqual(true, sutClassSelect[2].value);
        Assert.AreEqual(true, sutClassSelect[3].value);
    }
}
