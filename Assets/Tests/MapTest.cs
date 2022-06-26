using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;
using System.Linq;

public class MapTest
{
    private ResourceLoader resourceLoader;
    private GameInfo gameInfo;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        resourceLoader = Object.Instantiate(Resources.Load<ResourceLoader>("Prefabs/System/ResourceLoader"));
        gameInfo = Object.Instantiate(Resources.Load<GameInfo>("Prefabs/System/GameInfo"));
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Object.Destroy(resourceLoader.gameObject);
        Object.Destroy(gameInfo.gameObject);
    }

    [Test]
    public void _001_StairDownPlacingTest()
    {
        // setup
        int[] matrix =
        {
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 0, 2, 1, 1, 1, 4, 1,12, 1, 1, 1, 1,11, 2,
            6, 0, 2, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2,
           10, 0, 4, 1, 1,11, 2, 1,12, 1, 1, 1, 1, 1, 2,
            2, 4, 2, 4, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            5, 1,12, 1, 4, 0, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 4, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 7, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
        };

        var deadEnds = new Dictionary<Pos, IDirection>()
        {
            { new Pos(5, 5),    Direction.west  },
            { new Pos(1, 3),    Direction.north },
            { new Pos(1, 5),    Direction.north },
            { new Pos(13, 13),  Direction.north },
            { new Pos(1, 1),    Direction.south },
        };

        var firstDeadEndPos = deadEnds.First();
        Pos downStairsPos = firstDeadEndPos.Key;
        IDirection downStairsDir = firstDeadEndPos.Value;
        Pos inFrontOfDownStairsPos = downStairsDir.GetForward(downStairsPos);

        // when
        MapManager sut1 = new MapManager(1, matrix, 15, deadEnds);
        MapManager sut5 = new MapManager(5, matrix, 15, deadEnds); // Down stairs isn't set to last floor

        // then
        Assert.False(sut1.deadEndPos.ContainsKey(downStairsPos));
        Assert.AreEqual(Terrain.DownStairs, sut1.matrix[downStairsPos.x, downStairsPos.y]);
        Assert.AreEqual(downStairsDir.Enum, sut1.dirMap[downStairsPos.x, downStairsPos.y]);
        Assert.AreEqual(Terrain.Ground, sut1.matrix[inFrontOfDownStairsPos.x, inFrontOfDownStairsPos.y]);
        Assert.AreEqual(new KeyValuePair<Pos, IDirection>(inFrontOfDownStairsPos, downStairsDir), sut1.stairsTop);

        // then last floor
        Pos itemPlacedPos = downStairsPos;
        Pos inFrontOfItemPlacedPos = inFrontOfDownStairsPos;

        Assert.False(sut5.deadEndPos.ContainsKey(itemPlacedPos));
        Assert.AreEqual(Terrain.Path, sut5.matrix[itemPlacedPos.x, itemPlacedPos.y]);
        Assert.AreEqual(Dir.NES, sut5.dirMap[itemPlacedPos.x, itemPlacedPos.y]);
        Assert.AreEqual(Terrain.Door, sut5.matrix[inFrontOfItemPlacedPos.x, inFrontOfItemPlacedPos.y]);
        Assert.AreEqual(new KeyValuePair<Pos, IDirection>(new Pos(0, 0), null), sut5.stairsTop);
    }

    [Test]
    public void _002_1F_PitAttentionMessageBoardDirectionTest()
    {
        // setup
        // when
        MapManager sut = new MapManager(1);

        // then
        Assert.AreEqual(4, sut.pitTrapPos.Count);

        bool isExitDoorFound = false;
        sut.fixedMessagePos.ForEach(pos =>
        {
            IDirection messageDir = Direction.Convert(sut.dirMap[pos.x, pos.y]);
            Pos readPos = messageDir.GetForward(pos);

            Assert.That(sut.matrix[readPos.x, readPos.y], Is.EqualTo(Terrain.Path).Or.EqualTo(Terrain.Ground));

            Pos posL = messageDir.GetLeft(readPos);
            Pos posR = messageDir.GetRight(readPos);
            Terrain pitCandidateL = sut.matrix[posL.x, posL.y];
            Terrain pitCandidateR = sut.matrix[posR.x, posR.y];

            // Skip message board for exit door
            if (pitCandidateL == Terrain.ExitDoor || pitCandidateR == Terrain.ExitDoor)
            {
                isExitDoorFound = true;
                return;
            }

            Assert.That(Terrain.Pit, Is.EqualTo(pitCandidateL).Or.EqualTo(pitCandidateR));
        });

        Assert.True(isExitDoorFound);
    }

    [Test]
    public void _003_1F_ExitDoorAndMessageBoardPlacingTest()
    {
        // setup
        // when
        MapManager sut = new MapManager(1, 49, 49);

        // then
        Pos startPos = sut.stairsBottom.Key;
        IDirection startDir = sut.stairsBottom.Value;

        Assert.False(sut.deadEndPos.ContainsKey(startPos));

        Pos posF = startDir.GetForward(startPos);
        Pos posL = startDir.GetLeft(startPos);
        Pos posR = startDir.GetRight(startPos);
        Pos posB = startDir.GetBackward(startPos);

        Assert.That(sut.matrix[posF.x, posF.y], Is.EqualTo(Terrain.Path).Or.EqualTo(Terrain.Ground));

        if (sut.matrix[posL.x, posL.y] == Terrain.ExitDoor)
        {
            Assert.True(posL.x == 0 || posL.y == 0 || posL.x == 48 || posL.y == 48);
            Assert.AreEqual(startDir.Right.Enum, sut.dirMap[posL.x, posL.y]);
            Assert.AreEqual(Terrain.MessageWall, sut.matrix[posB.x, posB.y]);
            Assert.AreEqual(startDir.Enum, sut.dirMap[posB.x, posB.y]);
        }
        else if (sut.matrix[posR.x, posR.y] == Terrain.ExitDoor)
        {
            Assert.True(posR.x == 0 || posR.y == 0 || posR.x == 48 || posR.y == 48);
            Assert.AreEqual(startDir.Left.Enum, sut.dirMap[posR.x, posR.y]);
            Assert.AreEqual(Terrain.MessageWall, sut.matrix[posB.x, posB.y]);
            Assert.AreEqual(startDir.Enum, sut.dirMap[posB.x, posB.y]);
        }
        else if (sut.matrix[posB.x, posB.y] == Terrain.ExitDoor)
        {
            Assert.True(posB.x == 0 || posB.y == 0 || posB.x == 48 || posB.y == 48);
            Assert.AreEqual(startDir.Enum, sut.dirMap[posB.x, posB.y]);
            Assert.AreEqual(Terrain.MessageWall, sut.matrix[posL.x, posL.y]);
            Assert.AreEqual(startDir.Right.Enum, sut.dirMap[posL.x, posL.y]);
        }
        else
        {
            // Exit Door isn't found.
            Assert.Fail();
        }
    }

    [Test]
    /// <summary>
    /// Pit traps are never placed in front of Items.
    /// </summary>
    public void _004_PitPlacingTest()
    {
        // setup
        MapManager[] sut = new MapManager[5];

        // when
        for (int floor = 1; floor <= 5; floor++)
        {
            sut[floor - 1] = new MapManager(floor, 49, 49);
        }

        // then
        sut.ForEach(sutFloor =>
        {
            sutFloor.deadEndPos.ForEach(kv =>
            {
                Pos inFrontOfDeadEnd = kv.Value.GetForward(kv.Key);
                Assert.AreNotEqual(Terrain.Pit, sutFloor.matrix[inFrontOfDeadEnd.x, inFrontOfDeadEnd.y]);
            });
        });
    }
}
