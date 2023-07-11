using NUnit.Framework;
using UnityEngine;
using UniRx;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Object = UnityEngine.Object;
using Moq;

public class ItemTest
{
    private ResourceLoader resourceLoader;
    private GameInfo gameInfo;
    private GameManagerTest gameManager;
    private ItemGenerator itemGenerator;
    private Camera mainCamera;

    private PlayerInfoTest playerInfo;
    private PlayerInfoTest prefabPlayerInfo;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        resourceLoader = Object.Instantiate(Resources.Load<ResourceLoader>("Prefabs/System/ResourceLoader"));
        gameInfo = Object.Instantiate(Resources.Load<GameInfo>("Prefabs/System/GameInfo"));
        prefabPlayerInfo = Resources.Load<PlayerInfoTest>("Prefabs/Character/Player/PlayerInfoDummy");
        gameManager = Object.Instantiate(Resources.Load<GameManagerTest>("Prefabs/System/GameManagerTest"), Vector3.zero, Quaternion.identity); ;
        itemGenerator = Object.Instantiate(Resources.Load<ItemGenerator>("Prefabs/Map/ItemGenerator"), Vector3.zero, Quaternion.identity); ;
        mainCamera = Object.Instantiate(Resources.Load<Camera>("Prefabs/UI/MainCamera"));
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Object.Destroy(gameManager.gameObject);
        Object.Destroy(resourceLoader.gameObject);
        Object.Destroy(gameInfo.gameObject);
        Object.Destroy(itemGenerator.gameObject);
        Object.Destroy(mainCamera.gameObject);
    }

    [SetUp]
    public void SetUp()
    {
        GameInfo.Instance.isScenePlayedByEditor = false;
        playerInfo = Object.Instantiate(prefabPlayerInfo);

        var mock = new Mock<IPlayerMapUtil>();
        mock.Setup(x => x.dir).Returns(Direction.north);
        mock.Setup(x => x.Dir).Returns(Observable.Return(Direction.north));
        playerInfo.SetMapUtil(mock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(playerInfo.gameObject);
    }

    [UnityTest]
    public IEnumerator _001_VerifyFinalMapItemTest()
    {
        // setup
        var mapData = gameInfo.FinalMap();
        var map = WorldMap.Create(mapData);

        var source = ResourceLoader.Instance.itemTypesData.Param(gameInfo.LastFloor - 1);

        yield return null;

        itemGenerator.PlaceItems(map);

        yield return null;

        var matrix = map.matrix.Clone() as ITile[,];

        var fixedItemList = mapData.fixedItemPos.Keys.ToList();

        for (int i = 0; i < fixedItemList.Count; ++i)
        {
            Pos pos = fixedItemList[i];

            // Terrain of fixed item position must be Box.
            Assert.AreEqual(mapData.matrix[pos.x, pos.y], Terrain.Box, $"pos = ({pos.x}, {pos.y})");

            // Tile type of fixed item position must be Box.
            Assert.True(matrix[pos.x, pos.y] is Box, $"pos = ({pos.x}, {pos.y})");

            // Fixed item position has an determined item stored reversely in fixed item type list.
            Assert.AreEqual(matrix[pos.x, pos.y].PickItem()?.itemInfo?.type, source.fixedTypes[i], $"pos = ({pos.x}, {pos.y})");
        }

        var randomItemList = new List<Pos>(mapData.randomItemPos);

        for (int j = 0; j < mapData.height; ++j)
        {
            for (int i = 0; i < mapData.width; ++i)
            {
                if (mapData.matrix[i, j] == Terrain.Box)
                {
                    // Tile type of fixed item position must be Box.
                    Assert.True(matrix[i, j] is Box, $"pos = ({i}, {j})");

                    Pos pos = new Pos(i, j);

                    // Skip if fixed item position.
                    if (fixedItemList.Contains(pos)) continue;

                    // Box position that is not fixed item position has one of the random items.
                    var boxType = matrix[i, j].PickItem()?.itemInfo?.type;
                    Assert.True(source.randomTypes.Contains(type => type == boxType), $"pos = ({i}, {j})");

                    // Box position that is not fixed item position is included in random item pos.
                    Assert.True(randomItemList.Contains(pos), $"pos = ({i}, {j})");

                    // Remove box position from random item position.
                    randomItemList.Remove(pos);
                }
            }
        }

        randomItemList.ForEach(pos =>
        {
            // Random item position has one of the random items.
            var randomType = matrix[pos.x, pos.y].PickItem()?.itemInfo?.type;
            Assert.True(source.randomTypes.Contains(type => type == randomType), $"pos = ({pos.x}, {pos.y})");
        });
    }

    [Test]
    public void _002_VerifyFinalMapBoxDirectionTest()
    {
        // setup
        var mapData = gameInfo.FinalMap();
        var map = WorldMap.Create(mapData);

        var source = ResourceLoader.Instance.itemTypesData.Param(gameInfo.LastFloor - 1);

        var boxPos = new Dictionary<Pos, IDirection>()
        {
            { new Pos(12, 12),  Direction.north     },
            { new Pos(12, 16),  Direction.north     },
            { new Pos(16, 12),  Direction.north     },
            { new Pos(16, 16),  Direction.north     },
            { new Pos( 6, 13),  Direction.east      },
            { new Pos( 5, 14),  Direction.west      },
            { new Pos( 6, 15),  Direction.east      },
            { new Pos( 5, 16),  Direction.west      },
            { new Pos( 6, 17),  Direction.east      },
        };

        var dirMap = map.dirMapHandler.CloneDirMap();
        var matrix = map.dirMapHandler.CloneMatrix();

        boxPos.ForEach(kv =>
        {
            Assert.True(map.matrix[kv.Key.x, kv.Key.y] is Box);
            Assert.AreEqual(matrix[kv.Key.x, kv.Key.y], Terrain.Box);
            Assert.AreEqual(Direction.Convert(dirMap[kv.Key.x, kv.Key.y]), kv.Value);
        });
    }
}
