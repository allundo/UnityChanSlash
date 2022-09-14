using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using System.Linq;
using System.Collections.Generic;
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

        // Keep data files

        fileNames = new string[] {
            dataStoreAgent.DEAD_RECORD_FILE_NAME,
            dataStoreAgent.CLEAR_RECORD_FILE_NAME,
            dataStoreAgent.INFO_RECORD_FILE_NAME,
            dataStoreAgent.SAVE_DATA_FILE_NAME
        };
        tempFiles = fileNames.Select(name => LoadText(name)).ToArray();
    }

    private string LoadText(string fileName)
    {
        try
        {
            return dataStoreAgent.LoadText(fileName);
        }
        catch (Exception e)
        {
            Debug.Log("failed to open file: " + e.Message);
            return "";
        }
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        // Restore data files
        for (int i = 0; i < fileNames.Length; i++)
        {
            dataStoreAgent.SaveText(fileNames[i], tempFiles[i]);
        }

        UnityEngine.Object.Destroy(dataStoreAgent.gameObject);
        UnityEngine.Object.Destroy(gameInfo.gameObject);
    }

    [Test]
    public void _001_DeadRecordSaveAndLoadTest()
    {
        // setup
        LogAssert.Expect(LogType.Error, new Regex($"ファイルのロードに失敗: Could not find file \".*/{dataStoreAgent.DEAD_RECORD_FILE_NAME}\""));

        dataStoreAgent.DeleteFile(dataStoreAgent.DEAD_RECORD_FILE_NAME);
        dataStoreAgent.SaveDeadRecords(new Attacker(1f, null, "test"), 10000, 3);

        var records = dataStoreAgent.LoadDeadRecords();

        Assert.AreEqual(10000, records[0].moneyAmount);
        Assert.AreEqual(3, records[0].floor);
        Assert.AreEqual("testにやられた", records[0].causeOfDeath);
        Assert.AreEqual("1,3,10000,testにやられた", string.Join(",", records[0].GetValues(1).Select(obj => obj.ToString())));
    }


    [Test]
    public void _002_SaveGameDataAndLoadTest()
    {
        dataStoreAgent.DeleteFile(dataStoreAgent.SAVE_DATA_FILE_NAME);

        var saveData = new DataStoreAgent.SaveData()
        {
            currentFloor = 1,
            elapsedTimeSec = 1000,
            playerData = new DataStoreAgent.MobData(new Pos(1, 1), Direction.north, 12f, false, true),
            mapData = new DataStoreAgent.MapData[] { new DataStoreAgent.MapData(new WorldMap()), null, null, null, null },
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
        Assert.False(loadData.playerData.isIced);
        Assert.True(loadData.playerData.isHidden);
        Assert.AreEqual(13, loadData.respawnData[0].itemData[0].pos.x);
        Assert.AreEqual(32, loadData.respawnData[0].itemData[0].pos.y);
        Assert.AreEqual(3, loadData.respawnData[0].itemData[0].itemType);
        Assert.AreEqual(2, loadData.respawnData[0].itemData[0].numOfItem);

        Assert.AreEqual(mapData.stairsBottom.Convert(), map.StairsBottom);
        Assert.AreEqual(mapData.stairsTop.Convert(), map.stairsTop);

        for (int y = 0; y < 49; y++)
        {
            for (int x = 0; x < 49; x++)
            {
                Assert.False(map.discovered[x, y]);
            }
        }
    }
}
