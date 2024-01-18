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
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,17, 2,
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
        WorldMap sut2 = WorldMap.Create(new CustomMapData(2, matrix, 15, deadEnds));
        WorldMap sut5 = WorldMap.Create(new CustomMapData(5, matrix, 15, deadEnds)); // Down stairs isn't set to last floor

        // then
        var matrix1 = sut2.dirMapHandler.CloneMatrix();
        var dirMap1 = sut2.dirMapHandler.CloneDirMap();
        Assert.AreEqual(Terrain.DownStairs, matrix1[downStairsPos.x, downStairsPos.y]);
        Assert.AreEqual(downStairsDir.Enum, dirMap1[downStairsPos.x, downStairsPos.y]);
        Assert.AreEqual(Terrain.Ground, matrix1[inFrontOfDownStairsPos.x, inFrontOfDownStairsPos.y]);
        Assert.AreEqual(inFrontOfDownStairsPos, sut2.stairsTop.Key);
        Assert.AreEqual(downStairsDir, sut2.stairsTop.Value);

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
            Pos pos = fixedMessagePos[i];

            // Skip if not pit trap message
            string name = (sut.GetTile(pos) as MessageWall).data.Source[0].name;
            if (!name.StartsWith("落とし穴"))
            {
                Debug.Log($"Skip board: {name}");
                continue;
            }

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
        WorldMap[] sut = new WorldMap[4];

        // when
        for (int floor = 1; floor <= 4; floor++)
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
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,17, 2,
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

        FloorMessagesSource src2 = ResourceLoader.Instance.floorMessagesData.Param(1);
        FloorMessagesSource src5 = ResourceLoader.Instance.floorMessagesData.Param(4);
        int numOfFixedMessage2 = src2.fixedMessages.Length;
        int numOfBloodMessage2 = src2.bloodMessages.Length;
        int numOfFixedMessage5 = src5.fixedMessages.Length;
        int numOfBloodMessage5 = src5.bloodMessages.Length;

        // when
        WorldMap sut2 = WorldMap.Create(new CustomMapData(2, matrix, 15, boxItemPos, randomItemPos, fixedMessagePos, bloodMessagePos));
        WorldMap sut5 = WorldMap.Create(new CustomMapData(5, matrix, 15, boxItemPos, randomItemPos, fixedMessagePos, bloodMessagePos)); // Down stairs isn't set to last floor

        // then
        var mesData2 = sut2.messagePosData;
        var fixedMes2 = mesData2.fixedMessagePos;
        Assert.AreEqual(numOfFixedMessage2, fixedMes2.Count);
        Assert.AreEqual(new Pos(2, 2), fixedMes2[0]);
        Assert.AreEqual(new Pos(0, 8), fixedMes2[1]);
        Assert.AreEqual(new Pos(0, 10), fixedMes2[2]);
        Assert.AreEqual(new Pos(0, 12), fixedMes2[3]);

        Assert.AreEqual(numOfBloodMessage2, mesData2.bloodMessagePos.Count);

        // then last floor
        var mesData5 = sut5.messagePosData;
        Assert.AreEqual(numOfFixedMessage5, mesData5.fixedMessagePos.Count);
        Assert.AreEqual(new Pos(2, 2), mesData5.fixedMessagePos[0]);

        Assert.AreEqual(numOfBloodMessage5, mesData5.bloodMessagePos.Count);
        Assert.AreEqual(new Pos(14, 12), mesData5.bloodMessagePos[0]);
    }

    [Test]
    public void _007_SecretMessageBoardPlacingTest([Values(0, 1, 2, 4, 8)] int secretLevel)
    {
        // setup
        GameInfo info = GameInfo.Instance;
        info.secretLevel = secretLevel;

        int[] matrix =
        {
            2, 2, 2, 7, 2, 7, 2, 7, 2, 2, 2, 2, 2, 2, 2,
            2, 0, 2, 1, 1, 1, 4, 1,22, 1, 1, 1, 1,21, 2,
            6, 0, 2, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2,
           20, 0, 4, 1, 1,21, 2, 1,22, 1, 1, 1, 1, 1, 2,
            2, 4, 2, 4, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            5, 1,22, 1, 4, 0, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 7, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            7, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            7, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 4, 2,
            7, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2,
            7, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
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

        // when
        var floorMessagesData = ResourceLoader.Instance.floorMessagesData;
        var sut = Enumerable.Range(1, info.LastFloor).Select(floor =>
            floor == 1
                ? WorldMap.Create()
                : WorldMap.Create(new CustomMapData(floor, matrix, 15, null, null, fixedMessagePos, bloodMessagePos))
            ).ToArray();

        var maxNumOfSecret = Enumerable.Range(0, info.LastFloor).Select(index => floorMessagesData.Param(index).secretMessages.Length).ToArray();

        // then
        var mesData = sut.Select(map => map.messagePosData).ToArray();

        var secret = sut.Select(map =>
        {
            var list = new List<SecretMessageData>();
            map.ForEachTiles(tile =>
            {
                MessageWall mes = tile as MessageWall;
                if (mes != null && mes.data is SecretMessageData) list.Add(mes.data as SecretMessageData);
            });
            return list;
        }).ToArray();

        for (int j = 0; j < info.LastFloor; ++j)
        {
            for (int i = 0; i < info.LastFloor; ++i)
            {
                int numOfSecret = secret[j].Count(data => data.messageID >= i * 10 && data.messageID < (i + 1) * 10);
                Debug.Log($"Floor: {j + 1}, number of secret{i + 1} = {numOfSecret}");
                if (i > j) Assert.AreEqual(0, numOfSecret);
            }

            int secretTileCount = secret[j].Count();

            int fixedPosCount = mesData[j].fixedMessagePos.Count;
            int bloodPosCount = mesData[j].bloodMessagePos.Count;
            int randomPosCount = mesData[j].randomMessagePos.Count;
            int secretPosCount = mesData[j].secretMessagePos.Count;

            Debug.Log($"Floor: {j + 1}, number of random message pos = {randomPosCount}");

            var convertData = CustomMapData.RetrieveData(mesData[j]);
            int messageCount = convertData.randomMes.Count; // All of placed message boards are counted as randomMes
            int bloodCount = convertData.secretMes.Count; // All of placed blood message boards are counted as secretMes

            Assert.AreEqual(secretTileCount, secretPosCount);
            Assert.That(secretTileCount, Is.LessThanOrEqualTo(maxNumOfSecret[j]));
            Assert.That(secretTileCount, Is.LessThanOrEqualTo(secretLevel + 1));

            Assert.AreEqual(messageCount, fixedPosCount + randomPosCount);
            Assert.AreEqual(bloodCount, bloodPosCount + secretPosCount);
            Assert.AreEqual(convertData.roomCenter.Count, 0);
        }
    }

    [Test]
    public void _008_VerifySecretMessageBoardOnCreatingMapTest([Values(0, 1, 2, 4, 8)] int secretLevel)
    {
        // setup
        GameInfo info = GameInfo.Instance;
        info.secretLevel = secretLevel;

        // when
        var floorMessagesData = ResourceLoader.Instance.floorMessagesData;
        var mapSize = new int[] { 19, 31, 49, 49, 29 };
        var sut = Enumerable.Range(1, info.LastFloor).Select(floor => WorldMap.Create(floor, mapSize[floor - 1], mapSize[floor - 1])).ToArray();

        var maxNumOfSecret = Enumerable.Range(0, info.LastFloor).Select(index => floorMessagesData.Param(index).secretMessages.Length).ToArray();

        // then
        var mesData = sut.Select(map => map.messagePosData).ToArray();

        var secret = sut.Select(map =>
        {
            var list = new List<SecretMessageData>();
            map.ForEachTiles(tile =>
            {
                MessageWall mes = tile as MessageWall;
                if (mes != null && mes.data is SecretMessageData) list.Add(mes.data as SecretMessageData);
            });
            return list;
        }).ToArray();

        for (int j = 0; j < info.LastFloor; ++j)
        {
            for (int i = 0; i < info.LastFloor; ++i)
            {
                int numOfSecret = secret[j].Count(data => data.messageID >= i * 10 && data.messageID < (i + 1) * 10);
                Debug.Log($"Floor: {j + 1}, number of secret{i + 1} = {numOfSecret}");
                if (i > j) Assert.AreEqual(0, numOfSecret);
            }

            int secretTileCount = secret[j].Count();

            int fixedPosCount = mesData[j].fixedMessagePos.Count;
            int bloodPosCount = mesData[j].bloodMessagePos.Count;
            int randomPosCount = mesData[j].randomMessagePos.Count;
            int secretPosCount = mesData[j].secretMessagePos.Count;

            Debug.Log($"Floor: {j + 1}, number of random message pos = {randomPosCount}");

            var convertData = CustomMapData.RetrieveData(mesData[j]);
            int messageCount = convertData.randomMes.Count; // All of placed message boards are counted as randomMes
            int bloodCount = convertData.secretMes.Count; // All of placed blood message boards are counted as secretMes

            Assert.AreEqual(secretTileCount, secretPosCount);
            Assert.That(secretTileCount, Is.LessThanOrEqualTo(maxNumOfSecret[j]));
            Assert.That(secretTileCount, Is.LessThanOrEqualTo(secretLevel + 1));

            Assert.AreEqual(messageCount, fixedPosCount + randomPosCount);
            Assert.AreEqual(bloodCount, bloodPosCount + secretPosCount);
            Assert.AreEqual(convertData.roomCenter.Count, 0);
        }
    }

    [Test]
    public void _009_VerifyNumOfPitAttentionsTest()
    {
        var floor1 = WorldMap.Create(1, 7, 7);

        ITile[,] matTile = floor1.matrix.Clone() as ITile[,];
        Terrain[,] matTerrain = floor1.dirMapHandler.CloneMatrix();

        int pitCount = 0;
        int pitAttentionCount = 0;

        for (int j = 0; j < 7; ++j)
        {
            for (int i = 0; i < 7; ++i)
            {
                var message = matTile[i, j] as MessageWall;
                if (message != null)
                {
                    if (message.data.Source[0].name == "落とし穴注意")
                    {
                        ++pitAttentionCount;
                    }
                }

                if (matTerrain[i, j] == Terrain.Pit) ++pitCount;
            }
        }

        Pos pitMes = floor1.messagePosData.fixedMessagePos[pitAttentionCount];
        Pos notPitMes = floor1.messagePosData.fixedMessagePos[pitAttentionCount + 1];
        string pitMesName = (matTile[pitMes.x, pitMes.y] as MessageWall).data.Source[0].name;
        string notPitMesName = (matTile[notPitMes.x, notPitMes.y] as MessageWall).data.Source[0].name;

        Assert.LessOrEqual(pitAttentionCount, pitCount);
        Assert.AreEqual("落とし穴注意", pitMesName);
        Assert.AreNotEqual("落とし穴注意", notPitMesName);
    }
}
