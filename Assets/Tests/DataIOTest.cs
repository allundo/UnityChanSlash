using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using System.Text.RegularExpressions;

public class DataIOTest
{
    private DataStoreAgentTest dataStoreAgent;
    private string[] tempFiles;
    private string[] fileNames;
    private GameInfo gameInfo;
    private ResourceLoader resourceLoader;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        dataStoreAgent = UnityEngine.Object.Instantiate(Resources.Load<DataStoreAgentTest>("Prefabs/System/DataStoreAgentTest"), Vector3.zero, Quaternion.identity); ;
        resourceLoader = UnityEngine.Object.Instantiate(Resources.Load<ResourceLoader>("Prefabs/System/ResourceLoader"), Vector3.zero, Quaternion.identity); ;
        gameInfo = UnityEngine.Object.Instantiate(Resources.Load<GameInfo>("Prefabs/System/GameInfo"), Vector3.zero, Quaternion.identity); ;

        dataStoreAgent.KeepSaveDataFiles();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        dataStoreAgent.RestoreSaveDataFiles();

        UnityEngine.Object.Destroy(dataStoreAgent.gameObject);
        UnityEngine.Object.Destroy(resourceLoader.gameObject);
        UnityEngine.Object.Destroy(gameInfo.gameObject);
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

        var floor1Map = new WorldMap();
        var stairsBottom = floor1Map.StairsBottom;
        var stairsTop = floor1Map.stairsTop;

        var counter = new PlayerCounter();

        counter.IncDefeat(EnemyType.RedSlime);
        counter.IncDefeat(EnemyType.SkeletonSoldier);
        counter.IncDefeat(EnemyType.None);
        counter.IncDefeat(EnemyType.RedSlime);
        counter.IncAttack();
        counter.IncAttack();
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

        counter.TotalCounts();

        counter.IncDefeat(EnemyType.SkeletonSoldier);
        counter.IncAttack();
        counter.IncAttack();
        counter.IncShield();
        counter.IncMagic(AttackAttr.Dark);
        counter.IncDamage();
        counter.IncMagicDamage();

        counter.IncMagic(AttackAttr.Coin);
        counter.IncMagic(AttackAttr.Coin);
        counter.IncPotion();

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

        Assert.AreEqual(stairsBottom, map.StairsBottom);
        Assert.AreEqual(stairsTop, map.stairsTop);
        Assert.AreEqual(mapData.stairsBottom.Convert(), map.StairsBottom);
        Assert.AreEqual(mapData.stairsTop.Convert(), map.stairsTop);

        Assert.AreEqual(1, loadData.playerData.counter.Defeat);
        Assert.AreEqual(2, loadData.playerData.counter.Attack);
        Assert.AreEqual(1, loadData.playerData.counter.Shield);
        Assert.AreEqual(1, loadData.playerData.counter.Magic);
        Assert.AreEqual(1, loadData.playerData.counter.Damage);
        Assert.AreEqual(1, loadData.playerData.counter.MagicDamage);

        Assert.AreEqual(3, loadData.playerData.counter.DefeatSum);
        Assert.AreEqual(2, loadData.playerData.counter.DefeatType(EnemyType.RedSlime));
        Assert.AreEqual(2, loadData.playerData.counter.AttackSum);
        Assert.AreEqual(1, loadData.playerData.counter.ShieldSum);
        Assert.AreEqual(3, loadData.playerData.counter.MagicSum);
        Assert.AreEqual(1, loadData.playerData.counter.DamageSum);
        Assert.AreEqual(1, loadData.playerData.counter.MagicDamageSum);
        Assert.AreEqual(2, loadData.playerData.counter.CoinSum);
        Assert.AreEqual(3, loadData.playerData.counter.PotionSum);

        Assert.AreEqual(LevelGainType.Attacker, loadData.playerData.levelGainType);

        for (int y = 0; y < 49; y++)
        {
            for (int x = 0; x < 49; x++)
            {
                Assert.False(map.discovered[x, y]);
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
}
