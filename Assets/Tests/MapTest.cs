using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;
using System.Linq;

public class MapTest
{
    [OneTimeSetUp]
    public void SetUp()
    {
        Object.Instantiate(Resources.Load<ResourceLoader>("Prefabs/System/ResourceLoader"));
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
        MapManager sut = new MapManager().SetDownStairs();

        // when
        sut.SetPitAndMessageBoards(1);

        // then
        Assert.AreEqual(4, sut.pitTrapPos.Count);
        Assert.AreEqual(4, sut.fixedMessagePos.Count);

        sut.fixedMessagePos.ForEach(pos =>
        {
            IDirection messageDir = Direction.Convert(sut.dirMap[pos.x, pos.y]);
            Pos readPos = messageDir.GetForward(pos);

            Assert.That(sut.matrix[readPos.x, readPos.y], Is.EqualTo(Terrain.Path).Or.EqualTo(Terrain.Ground));

            Pos posL = messageDir.GetLeft(readPos);
            Pos posR = messageDir.GetRight(readPos);
            Terrain pitCandidateL = sut.matrix[posL.x, posL.y];
            Terrain pitCandidateR = sut.matrix[posR.x, posR.y];

            Assert.That(Terrain.Pit, Is.EqualTo(pitCandidateL).Or.EqualTo(pitCandidateR));
        });
    }
}
