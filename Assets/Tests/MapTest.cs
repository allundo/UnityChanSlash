using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;
using System.Linq;
using System;
using Object = UnityEngine.Object;

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
            2,11, 2, 1, 1, 1, 4, 1,22, 1, 1, 1, 1,21, 2,
            6, 0, 2, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2,
           20, 0, 4, 1, 1,21, 2, 1,22, 1, 1, 1, 1, 1, 2,
            2, 4, 2, 4, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            5, 1,22, 1, 4,10, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 4, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
        };

        var deadEnds = new Dictionary<Pos, IDirection>()
        {
            { new Pos(1, 3),    Direction.north },
            { new Pos(1, 5),    Direction.north },
            { new Pos(13, 13),  Direction.north },
        };

        Pos downStairsPos = new Pos(5, 5);
        IDirection downStairsDir = Direction.west;
        Pos inFrontOfDownStairsPos = downStairsDir.GetForward(downStairsPos);

        // when
        WorldMap sut1 = WorldMap.Create(new CustomMapData(1, matrix, 15, deadEnds));
        WorldMap sut5 = WorldMap.Create(new CustomMapData(5, matrix, 15, deadEnds)); // Down stairs isn't set to last floor

        // then
        var matrix1 = sut1.dirMapHandler.CloneMatrix();
        var dirMap1 = sut1.dirMapHandler.CloneDirMap();
        Assert.AreEqual(Terrain.DownStairs, matrix1[downStairsPos.x, downStairsPos.y]);
        Assert.AreEqual(downStairsDir.Enum, dirMap1[downStairsPos.x, downStairsPos.y]);
        Assert.AreEqual(Terrain.Ground, matrix1[inFrontOfDownStairsPos.x, inFrontOfDownStairsPos.y]);
        Assert.AreEqual(inFrontOfDownStairsPos, sut1.stairsTop.Key);
        Assert.AreEqual(downStairsDir, sut1.stairsTop.Value);

        // then last floor
        Pos itemPlacedPos = downStairsPos;
        Pos inFrontOfItemPlacedPos = inFrontOfDownStairsPos;

        var matrix5 = sut5.dirMapHandler.CloneMatrix();
        var dirMap5 = sut5.dirMapHandler.CloneDirMap();
        Assert.AreEqual(Terrain.Path, matrix5[itemPlacedPos.x, itemPlacedPos.y]);
        Assert.AreEqual(Dir.NES, dirMap5[itemPlacedPos.x, itemPlacedPos.y]);
        Assert.AreEqual(Terrain.Door, matrix5[inFrontOfItemPlacedPos.x, inFrontOfItemPlacedPos.y]);
        Assert.AreEqual(new Pos(), sut5.stairsTop.Key);
    }

    [Test]
    public void _002_1F_PitAttentionMessageBoardDirectionTest()
    {
        // setup
        int floor = 1;
        var fixedMessages = ResourceLoader.Instance.floorMessagesData.Param(floor - 1)
            .fixedMessages.Select(data => data.Convert()).ToList();

        // when
        WorldMap sut = WorldMap.Create(floor);

        // then
        int pitID = (int)Terrain.Pit;
        Assert.AreEqual(4, sut.dirMapHandler.ConvertMapData().Where(id => id == pitID).Count());

        var matrix = sut.dirMapHandler.CloneMatrix();
        var dirMap = sut.dirMapHandler.CloneDirMap();

        int count = 0;
        var fixedMessagePos = sut.messagePosData.fixedMessagePos;
        for (int i = 0; i < fixedMessagePos.Count; ++i)
        {
            // Skip if not pit trap message
            string name = fixedMessages[i].Source[0].name;
            if (!name.StartsWith("落とし穴"))
            {
                Debug.Log($"Skip board: {name}");
                continue;
            }

            Pos pos = fixedMessagePos[i];

            IDirection messageDir = Direction.Convert(dirMap[pos.x, pos.y]);
            Pos readPos = messageDir.GetForward(pos);

            Assert.That(matrix[readPos.x, readPos.y], Is.EqualTo(Terrain.Path).Or.EqualTo(Terrain.Ground));

            Pos posL = messageDir.GetLeft(readPos);
            Pos posR = messageDir.GetRight(readPos);
            Terrain pitCandidateL = matrix[posL.x, posL.y];
            Terrain pitCandidateR = matrix[posR.x, posR.y];

            Assert.That(Terrain.Pit, Is.EqualTo(pitCandidateL).Or.EqualTo(pitCandidateR));
            ++count;
        }

        Assert.AreEqual(4, count, $"Verified pit attentions count: {count}");
    }

    [Test]
    public void _003_1F_ExitDoorAndMessageBoardPlacingTest()
    {
        // setup
        // when
        WorldMap sut = WorldMap.Create(1, 49, 49);

        // then
        Pos startPos = sut.stairsBottom.Key;
        IDirection startDir = sut.stairsBottom.Value;

        Pos posF = startDir.GetForward(startPos);
        Pos posL = startDir.GetLeft(startPos);
        Pos posR = startDir.GetRight(startPos);
        Pos posB = startDir.GetBackward(startPos);

        var matrix = sut.dirMapHandler.CloneMatrix();
        var dirMap = sut.dirMapHandler.CloneDirMap();

        Assert.That(matrix[posF.x, posF.y], Is.EqualTo(Terrain.Path).Or.EqualTo(Terrain.Ground));

        if (matrix[posL.x, posL.y] == Terrain.ExitDoor)
        {
            Assert.True(posL.x == 0 || posL.y == 0 || posL.x == 48 || posL.y == 48);
            Assert.AreEqual(startDir.Right.Enum, dirMap[posL.x, posL.y]);
            Assert.AreEqual(Terrain.MessageWall, matrix[posB.x, posB.y]);
            Assert.AreEqual(startDir.Enum, dirMap[posB.x, posB.y]);
        }
        else if (matrix[posR.x, posR.y] == Terrain.ExitDoor)
        {
            Assert.True(posR.x == 0 || posR.y == 0 || posR.x == 48 || posR.y == 48);
            Assert.AreEqual(startDir.Left.Enum, dirMap[posR.x, posR.y]);
            Assert.AreEqual(Terrain.MessageWall, matrix[posB.x, posB.y]);
            Assert.AreEqual(startDir.Enum, dirMap[posB.x, posB.y]);
        }
        else if (matrix[posB.x, posB.y] == Terrain.ExitDoor)
        {
            Assert.True(posB.x == 0 || posB.y == 0 || posB.x == 48 || posB.y == 48);
            Assert.AreEqual(startDir.Enum, dirMap[posB.x, posB.y]);
            Assert.AreEqual(Terrain.MessageWall, matrix[posL.x, posL.y]);
            Assert.AreEqual(startDir.Right.Enum, dirMap[posL.x, posL.y]);
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
        WorldMap[] sut = new WorldMap[5];

        // when
        for (int floor = 1; floor <= 5; floor++)
        {
            sut[floor - 1] = WorldMap.Create(floor, 49, 49);
        }

        // then
        sut.ForEach(sutFloor =>
        {
            var matrix = sutFloor.dirMapHandler.CloneMatrix();
            sutFloor.dirMapHandler.rawMapData.SearchDeadEnds().ForEach(kv =>
            {
                Pos inFrontOfDeadEnd = kv.Value.GetForward(kv.Key);
                Assert.AreNotEqual(Terrain.Pit, matrix[inFrontOfDeadEnd.x, inFrontOfDeadEnd.y]);
            });
        });
    }

    [Test]
    public void _005_1F_GeneratedMapMustHaveExitDoorCandidateAsStartPositionTest()
    {
        for (int i = 0; i < 1000; i++)
        {
            try
            {
                WorldMap.Create(1, 19, 19);
            }
            catch (Exception e)
            {
                throw new Exception("Generating map failed, count: " + i + ", -> " + e.Message + "\n" + e.StackTrace);
            }
        }
    }

    [Test]
    public void _006_MessageBoardPlacingTest()
    {
        // setup
        int[] matrix =
        {
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 0, 2, 1, 1, 1, 4, 1,22, 1, 1, 1, 1,21, 2,
            6, 0, 2, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2,
           20, 0, 4, 1, 1,21, 2, 1,22, 1, 1, 1, 1, 1, 2,
            2, 4, 2, 4, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            5, 1,22, 1, 4, 0, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 4, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
        };

        var randomItemPos = new List<Pos>()
        {
            new Pos(1, 3),
            new Pos(1, 5),
        };

        var boxItemPos = new Dictionary<Pos, IDirection>()
        {
            { new Pos(13, 13),  Direction.north },
        };

        var fixedMessagePos = new Dictionary<Pos, IDirection>()
        {
            { new Pos(2, 2),    Direction.west  },
            { new Pos(0, 8),    Direction.east  },
            { new Pos(0, 10),   Direction.east  },
            { new Pos(0, 12),   Direction.east  },
            { new Pos(2, 14),   Direction.north },
            { new Pos(9, 0),    Direction.south },
        };

        var bloodMessagePos = new Dictionary<Pos, IDirection>()
        {
            { new Pos(14, 12),    Direction.east },
            { new Pos(14, 11),    Direction.east },
        };

        FloorMessagesSource src1 = ResourceLoader.Instance.floorMessagesData.Param(0);
        FloorMessagesSource src5 = ResourceLoader.Instance.floorMessagesData.Param(4);
        int numOfFixedMessage1 = src1.fixedMessages.Length;
        int numOfBloodMessage1 = src1.bloodMessages.Length;
        int numOfFixedMessage5 = src5.fixedMessages.Length;
        int numOfBloodMessage5 = src5.bloodMessages.Length;

        // when
        WorldMap sut1 = WorldMap.Create(new CustomMapData(1, matrix, 15, boxItemPos, randomItemPos, fixedMessagePos, bloodMessagePos));
        WorldMap sut5 = WorldMap.Create(new CustomMapData(5, matrix, 15, boxItemPos, randomItemPos, fixedMessagePos, bloodMessagePos)); // Down stairs isn't set to last floor

        // then
        var mesData1 = sut1.messagePosData;
        var fixedMes1 = mesData1.fixedMessagePos;
        Assert.AreEqual(numOfFixedMessage1, fixedMes1.Count);
        Assert.AreEqual(new Pos(2, 2), fixedMes1[0]);
        Assert.AreEqual(new Pos(0, 8), fixedMes1[1]);
        Assert.AreEqual(new Pos(0, 10), fixedMes1[2]);
        Assert.AreEqual(new Pos(0, 12), fixedMes1[3]);
        Assert.AreEqual(new Pos(2, 14), fixedMes1[4]);

        Assert.AreEqual(numOfBloodMessage1, mesData1.bloodMessagePos.Count);

        // then last floor
        var mesData5 = sut5.messagePosData;
        Assert.AreEqual(numOfFixedMessage5, mesData5.fixedMessagePos.Count);
        Assert.AreEqual(new Pos(2, 2), mesData5.fixedMessagePos[0]);

        Assert.AreEqual(numOfBloodMessage5, mesData5.bloodMessagePos.Count);
        Assert.AreEqual(new Pos(14, 12), mesData5.bloodMessagePos[0]);
    }
}
