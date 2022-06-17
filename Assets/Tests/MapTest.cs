using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;
using System.Linq;

public class MapTest
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Object.Instantiate(Resources.Load<ResourceLoader>("Prefabs/System/ResourceLoader"));
        Object.Instantiate(Resources.Load<GameInfo>("Prefabs/System/GameInfo"));
    }

    [Test]
    public void _001_StairDownPlacingTest()
    {
        // setup
        MapManager sut = new MapManager();
        var firstDeadEndPos = sut.deadEndPos.First();
        Pos downStairsPos = firstDeadEndPos.Key;
        IDirection downStairsDir = firstDeadEndPos.Value;
        Pos inFrontOfDownStairsPos = downStairsDir.GetForward(downStairsPos);

        // when
        sut.SetDownStairs();

        // then
        Assert.False(sut.deadEndPos.ContainsKey(downStairsPos));
        Assert.AreEqual(Terrain.DownStairs, sut.matrix[downStairsPos.x, downStairsPos.y]);
        Assert.AreEqual(downStairsDir.Enum, sut.dirMap[downStairsPos.x, downStairsPos.y]);
        Assert.AreEqual(Terrain.Ground, sut.matrix[inFrontOfDownStairsPos.x, inFrontOfDownStairsPos.y]);
        Assert.AreEqual(new KeyValuePair<Pos, IDirection>(inFrontOfDownStairsPos, downStairsDir), sut.stairsTop);
    }

    [Test]
    public void _002_1F_PitAttentionMessageBoardDirectionTest()
    {
        // setup
        MapManager sut = new MapManager();

        // when
        sut.InitMap(1);

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
        MapManager sut = new MapManager(49, 49);

        // when
        sut.InitMap(1);

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
        MapManager[] sut = new MapManager[5].Select(_ => new MapManager(49, 49)).ToArray();

        // when
        for (int floor = 1; floor <= 5; floor++)
        {
            sut[floor - 1].InitMap(floor);
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
