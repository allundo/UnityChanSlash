using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DataIOTest
{
    private Camera mainCamera;
    private DataStoreAgentTest dataStoreAgent;
    private GameInfo gameInfo;
    private ResourceLoader resourceLoader;
    private MapRenderer mapRenderer;
    private GameManagerTest gameManager;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        dataStoreAgent = Object.Instantiate(Resources.Load<DataStoreAgentTest>("Prefabs/System/DataStoreAgentTest"), Vector3.zero, Quaternion.identity); ;
        gameManager = Object.Instantiate(Resources.Load<GameManagerTest>("Prefabs/System/GameManagerTest"), Vector3.zero, Quaternion.identity); ;
        resourceLoader = Object.Instantiate(Resources.Load<ResourceLoader>("Prefabs/System/ResourceLoader"), Vector3.zero, Quaternion.identity); ;
        gameInfo = Object.Instantiate(Resources.Load<GameInfo>("Prefabs/System/GameInfo"), Vector3.zero, Quaternion.identity);
        mapRenderer = Object.Instantiate(Resources.Load<MapRenderer>("Prefabs/Map/Dungeon"), Vector3.zero, Quaternion.identity); ;
        mainCamera = Object.Instantiate(Resources.Load<Camera>("Prefabs/UI/MainCamera"));

        dataStoreAgent.KeepSaveDataFiles();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        dataStoreAgent.RestoreSaveDataFiles();

        Object.Destroy(dataStoreAgent.gameObject);
        Object.Destroy(gameManager.gameObject);
        Object.Destroy(resourceLoader.gameObject);
        Object.Destroy(gameInfo.gameObject);
        Object.Destroy(mapRenderer.gameObject);
        Object.Destroy(mainCamera.gameObject);
    }

    [Test]
    public void _001_DeadRecordSaveAndLoadTest()
    {
        // setup
        LogAssert.Expect(LogType.Error, new Regex($"ファイルのロードに失敗: Could not find file \".*/{dataStoreAgent.DEAD_RECORD_FILE_NAME}\""));

        dataStoreAgent.DeleteFile(dataStoreAgent.DEAD_RECORD_FILE_NAME);

        var newRecord = new DataStoreAgent.DeadRecord(10000, new Attacker(1f, null, "test").CauseOfDeath(), 3);
        dataStoreAgent.SaveDeadRecords(newRecord);

        var records = dataStoreAgent.LoadDeadRecords();

        Assert.AreEqual(10000, records[0].moneyAmount);
        Assert.AreEqual(3, records[0].floor);
        Assert.AreEqual("testにやられた", records[0].causeOfDeath);
        Assert.AreEqual("1,3,10000,testにやられた", string.Join(",", records[0].GetValues(1).Select(obj => obj.ToString())));

        dataStoreAgent.DeleteFile(dataStoreAgent.DEAD_RECORD_FILE_NAME);
    }

    [Test]
    public void _002_SaveGameDataAndLoadTest()
    {
        dataStoreAgent.DeleteFile(dataStoreAgent.SAVE_DATA_FILE_NAME);

        var floor1Map = WorldMap.Create();
        var stairsBottom = floor1Map.stairsBottom;
        var stairsTop = floor1Map.stairsTop;

        var counter = new PlayerCounter();

        counter.IncDefeat(EnemyType.RedSlime);
        counter.IncDefeat(EnemyType.SkeletonSoldier);
        counter.IncDefeat(EnemyType.None);
        counter.IncDefeat(EnemyType.RedSlime);
        counter.IncAttack(EquipmentCategory.Knuckle);
        counter.IncAttack(EquipmentCategory.Knuckle);
        counter.IncAttack(EquipmentCategory.Sword, true);
        counter.IncAttack(EquipmentCategory.Shield, true);
        counter.IncShield();
        counter.IncMagic(AttackAttr.Fire);
        counter.IncMagic(AttackAttr.Ice);
        counter.IncMagic(AttackAttr.Ice);
        counter.IncDamage();
        counter.IncMagicDamage();

        counter.IncMagic(AttackAttr.Coin);
        counter.IncMagic(AttackAttr.Coin);
        counter.IncPotion();
        counter.IncPotion();

        counter.IncStep();
        counter.IncStep();

        counter.TotalCounts();

        counter.IncDefeat(EnemyType.SkeletonSoldier);
        counter.IncAttack(EquipmentCategory.Sword);
        counter.IncAttack(EquipmentCategory.Shield);
        counter.IncAttack(EquipmentCategory.Sword, true);
        counter.IncShield();
        counter.IncMagic(AttackAttr.Dark);
        counter.IncDamage();
        counter.IncMagicDamage();

        counter.IncMagic(AttackAttr.Coin);
        counter.IncMagic(AttackAttr.Coin);
        counter.IncPotion();

        counter.IncStep();

        var saveData = new DataStoreAgent.SaveData()
        {
            currentFloor = 1,
            elapsedTimeSec = 1000,
            playerData = new DataStoreAgent.PlayerData(new Pos(1, 1), Direction.north, 12f, 3, 100f, false, 60f, ExitState.PitFall, counter, LevelGainType.Attacker),
            inventoryItems = new DataStoreAgent.ItemInfo[] { new DataStoreAgent.ItemInfo(ItemType.FireRing, 12), null, null, new DataStoreAgent.ItemInfo(ItemType.Potion, 9), null, null, null },
            mapData = new DataStoreAgent.MapData[] { new DataStoreAgent.MapData(floor1Map), null, null, null, null },
            respawnData = Enumerable.Repeat(new DataStoreAgent.RespawnData(new DataStoreAgent.EnemyData[0], new DataStoreAgent.ItemData[] { new DataStoreAgent.ItemData(new Pos(13, 32), ItemType.Coin, 2) }), 5).ToArray()
        };
        var mapData = saveData.mapData[0];

        dataStoreAgent.SaveEncryptedRecordTest(saveData, dataStoreAgent.SAVE_DATA_FILE_NAME);
        dataStoreAgent.ImportGameData();
        var loadData = dataStoreAgent.LoadGameData();

        var map = gameInfo.Map(1);

        Assert.AreEqual(1, gameInfo.currentFloor);
        Assert.AreEqual(1000, loadData.elapsedTimeSec);
        Assert.AreEqual(Direction.north, loadData.playerData.dir);
        Assert.AreEqual(new Pos(1, 1), loadData.playerData.pos);
        Assert.AreEqual(12f, loadData.playerData.life);
        Assert.AreEqual(3, loadData.playerData.level);
        Assert.AreEqual(100f, loadData.playerData.exp);
        Assert.AreEqual(60f, loadData.playerData.icingFrames);
        Assert.False(loadData.playerData.isHidden);
        Assert.AreEqual(ExitState.PitFall, loadData.playerData.exitState);
        Assert.AreEqual(13, loadData.respawnData[0].itemData[0].pos.x);
        Assert.AreEqual(32, loadData.respawnData[0].itemData[0].pos.y);
        Assert.AreEqual(ItemType.Coin, loadData.respawnData[0].itemData[0].itemType);
        Assert.AreEqual(2, loadData.respawnData[0].itemData[0].numOfItem);

        Assert.AreEqual(ItemType.FireRing, loadData.inventoryItems[0].itemType);
        Assert.AreEqual(12, loadData.inventoryItems[0].numOfItem);
        Assert.AreEqual(ItemType.Null, loadData.inventoryItems[2].itemType);
        Assert.AreEqual(0, loadData.inventoryItems[4].numOfItem);

        Assert.AreEqual(stairsBottom, map.stairsBottom);
        Assert.AreEqual(stairsTop, map.stairsTop);

        Assert.AreEqual(1, loadData.playerData.counter.Defeat);
        Assert.AreEqual(3, loadData.playerData.counter.AttackPoint);
        Assert.AreEqual(2, loadData.playerData.counter.ShieldPoint);
        Assert.AreEqual(1, loadData.playerData.counter.Magic);
        Assert.AreEqual(1, loadData.playerData.counter.Damage);
        Assert.AreEqual(1, loadData.playerData.counter.MagicDamage);

        Assert.AreEqual(3, loadData.playerData.counter.DefeatSum);
        Assert.AreEqual(2, loadData.playerData.counter.DefeatType(EnemyType.RedSlime));
        Assert.AreEqual(4, loadData.playerData.counter.AttackPointSum);
        Assert.AreEqual(3, loadData.playerData.counter.ShieldPointSum);
        Assert.AreEqual(3, loadData.playerData.counter.MagicSum);
        Assert.AreEqual(1, loadData.playerData.counter.DamageSum);
        Assert.AreEqual(1, loadData.playerData.counter.MagicDamageSum);
        Assert.AreEqual(2, loadData.playerData.counter.CoinSum);
        Assert.AreEqual(3, loadData.playerData.counter.PotionSum);
        Assert.AreEqual(3, loadData.playerData.counter.StepSum);

        Assert.AreEqual(LevelGainType.Attacker, loadData.playerData.levelGainType);

        for (int y = 0; y < 49; y++)
        {
            for (int x = 0; x < 49; x++)
            {
                Assert.False(map.miniMapData.discovered[x, y]);
            }
        }

        dataStoreAgent.DeleteFile(dataStoreAgent.SAVE_DATA_FILE_NAME);
    }

    [Test]
    public void _003_SaveSettingDataAndLoadTest()
    {
        dataStoreAgent.DeleteFile(dataStoreAgent.SETTING_DATA_FILE_NAME);

        var settingData = new DataStoreAgent.SettingData(0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f);

        dataStoreAgent.SaveEncryptedRecordTest(settingData, dataStoreAgent.SETTING_DATA_FILE_NAME);

        var loadData = dataStoreAgent.LoadSettingData();

        Assert.AreEqual(1f, loadData[UIType.None]);
        Assert.AreEqual(0.1f, loadData[UIType.EnemyGauge]);
        Assert.AreEqual(0.2f, loadData[UIType.AttackButton]);
        Assert.AreEqual(0.3f, loadData[UIType.AttackRegion]);
        Assert.AreEqual(0.4f, loadData[UIType.MoveButton]);
        Assert.AreEqual(0.5f, loadData[UIType.HandleButton]);

        dataStoreAgent.DeleteFile(dataStoreAgent.SETTING_DATA_FILE_NAME);
    }

    [UnityTest]
    public IEnumerator _004_SaveTileDataAndLoadTest()
    {
        var waitForEndOfFrame = new WaitForEndOfFrame();
        var waitForHalfSecond = new WaitForSeconds(0.5f);

        dataStoreAgent.DeleteFile(dataStoreAgent.SAVE_DATA_FILE_NAME);

        yield return waitForHalfSecond;

        var floor1Map = WorldMap.Create(1);

        mapRenderer.Render(floor1Map);

        yield return waitForHalfSecond;

        var tileOpenData = new Dictionary<Pos, bool>();
        var tileBrokenData = new Dictionary<Pos, bool>();
        var messageReadData = new Dictionary<Pos, bool>();

        floor1Map.ForEachTiles((tile, pos) =>
        {
            var openable = tile as IOpenable;
            var door = openable as Door;
            var readable = tile as IReadable;

            if (openable != null)
            {
                if (Util.Judge(2)) openable.Open();
                tileOpenData[pos] = openable.IsOpen;

                if (door != null)
                {
                    if (Util.Judge(2)) door.Break();
                    tileBrokenData[pos] = door.IsBroken;
                }
            }

            if (readable != null)
            {
                if (Util.Judge(2)) readable.Read();
                messageReadData[pos] = readable.IsRead;
            }
        });

        Pos pos = floor1Map.stairsMapData.exitDoor;
        ExitDoor exit = (floor1Map.GetTile(pos) as ExitDoor);
        if (Util.Judge(2)) exit.Unlock();
        bool isExitDoorLocked = exit.IsLocked;

        var saveData = new DataStoreAgent.SaveData()
        {
            currentFloor = 1,
            elapsedTimeSec = 1000,
            playerData = new DataStoreAgent.PlayerData(new Pos(1, 1), Direction.north, 12f, 3, 100f, false, 60f, ExitState.PitFall, new PlayerCounter(), LevelGainType.Attacker),
            inventoryItems = new DataStoreAgent.ItemInfo[] { new DataStoreAgent.ItemInfo(ItemType.FireRing, 12), null, null, new DataStoreAgent.ItemInfo(ItemType.Potion, 9), null, null, null },
            mapData = new DataStoreAgent.MapData[] { new DataStoreAgent.MapData(floor1Map), null, null, null, null },
            isExitDoorLocked = ITileStateData.isExitDoorLocked,
            respawnData = Enumerable.Repeat(new DataStoreAgent.RespawnData(new DataStoreAgent.EnemyData[0], new DataStoreAgent.ItemData[] { new DataStoreAgent.ItemData(new Pos(13, 32), ItemType.Coin, 2) }), 5).ToArray()
        };

        dataStoreAgent.SaveEncryptedRecordTest(saveData, dataStoreAgent.SAVE_DATA_FILE_NAME);

        yield return new WaitForSeconds(1f);

        GameInfo.Instance.ImportGameData(dataStoreAgent.LoadGameData(), dataStoreAgent.LoadInfoRecord());

        var importMap = GameInfo.Instance.Map(1);

        mapRenderer.SetActiveTerrains(false);
        yield return waitForEndOfFrame;

        mapRenderer.DestroyObjects();
        yield return waitForEndOfFrame;

        mapRenderer.LoadFloorMaterials(importMap); // Switch world map for MapRenderer
        yield return waitForEndOfFrame;

        mapRenderer.InitMeshes();
        yield return waitForEndOfFrame;

        var terrainMeshes = mapRenderer.SetUpTerrainMeshes(importMap.dirMapHandler);
        yield return waitForHalfSecond;

        mapRenderer.GenerateTerrain(terrainMeshes);
        yield return waitForEndOfFrame;

        mapRenderer.SwitchTerrainMaterials();
        yield return waitForEndOfFrame;

        mapRenderer.SetActiveTerrains(true);
        yield return waitForEndOfFrame;

        mapRenderer.ApplyTileState();
        yield return waitForEndOfFrame;

        importMap.ForEachTiles((tile, pos) =>
        {
            var openable = tile as IOpenable;
            var door = openable as Door;
            var readable = tile as IReadable;

            if (openable != null)
            {
                if (door != null)
                {
                    Assert.AreEqual(tileBrokenData[pos], door.IsBroken, $"door is broken: {door.IsBroken}, pos (x, y) = ({pos.x}, {pos.y})");
                    Assert.AreEqual(door.IsBroken || tileOpenData[pos], openable.IsOpen, $"door is broken: {door.IsBroken}, pos (x, y) = ({pos.x}, {pos.y})");
                }
                else
                {
                    Assert.AreEqual(tileOpenData[pos], openable.IsOpen, $"tile = {tile}, pos (x, y) = ({pos.x}, {pos.y})");
                }
            }
            if (readable != null) Assert.AreEqual(messageReadData[pos], readable.IsRead, $"tile = {tile}, pos (x, y) = ({pos.x}, {pos.y})");
        });

        Pos importPos = importMap.stairsMapData.exitDoor;
        Assert.AreEqual(isExitDoorLocked, (importMap.GetTile(importPos) as ExitDoor).IsLocked);

        dataStoreAgent.DeleteFile(dataStoreAgent.SETTING_DATA_FILE_NAME);
    }
}
