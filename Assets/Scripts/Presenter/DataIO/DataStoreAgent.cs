using UnityEngine;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

public class DataStoreAgent : SingletonMonoBehaviour<DataStoreAgent>
{
    public readonly string DEAD_RECORD_FILE_NAME = "dead_r.dat";
    public readonly string CLEAR_RECORD_FILE_NAME = "clear_r.dat";
    public readonly string INFO_RECORD_FILE_NAME = "info_r.dat";
    public readonly string SAVE_DATA_FILE_NAME = "save_data.dat";

    private class RecordSet
    {
        public string tag;
        public string hash;
        public byte[] nonce;
        public string data;
    }

    public abstract class DataArray
    {
        public abstract object[] GetValues();
        public virtual object[] GetValues(int rank)
        {
            var list = new List<object>() { rank };
            list.AddRange(GetValues());
            return list.ToArray();
        }
    }

    [System.Serializable]
    public class DeadRecord : DataArray
    {
        public DeadRecord(ulong moneyAmount, string causeOfDeath, int floor)
        {
            this.moneyAmount = moneyAmount;
            this.causeOfDeath = causeOfDeath;
            this.floor = floor;
        }

        public ulong moneyAmount = 0;
        public string causeOfDeath = "";
        public int floor = 1;

        public override object[] GetValues() => new object[] { floor, moneyAmount, causeOfDeath };
    }

    [System.Serializable]
    public class RecordList<T> where T : DataArray
    {
        public List<T> records;
        public RecordList(List<T> records)
        {
            this.records = records;
        }
    }

    [System.Serializable]
    public class ClearRecord : DataArray
    {
        public ClearRecord(string title, ulong wagesAmount, int clearTimeSec, int defeatCount)
        {
            this.title = title;
            this.wagesAmount = wagesAmount;
            this.clearTimeSec = clearTimeSec;
            this.defeatCount = defeatCount;
        }

        public string title = "なし";
        public ulong wagesAmount = 0;
        public int clearTimeSec = 0;
        public int defeatCount = 0;

        public override object[] GetValues() => new object[] { title, wagesAmount, clearTimeSec, defeatCount };
    }

    [System.Serializable]
    public class SaveData : DataArray
    {
        public int currentFloor = 0;
        public int elapsedTimeSec = 0;
        public PlayerData playerData = null;
        public MapData[] mapData = null;
        public RespawnData[] respawnData = null;
        public override object[] GetValues() => new object[] { };
    }

    [System.Serializable]
    public class MapData
    {
        public MapData(WorldMap map)
        {
            mapMatrix = map.GetMapMatrix();
            mapSize = map.Width;
            dirMap = map.GetDirData();

            var bottom = map.StairsBottom;
            var top = map.stairsTop;

            if (!bottom.Key.IsNull)
            {
                stairsBottomPosX = bottom.Key.x;
                stairsBottomPosY = bottom.Key.y;
                stairsBottomDir = (int)bottom.Value.Enum;
            }

            if (!top.Key.IsNull)
            {
                stairsTopPosX = top.Key.x;
                stairsTopPosY = top.Key.y;
                stairsTopDir = (int)top.Value.Enum;
            }

            tileOpenData = map.ExportTileOpenData().ToArray();
            roomCenterPos = map.roomCenterPos.ToArray();
            randomMessagePos = map.ExportRandomMessagePos();
            fixedMessagePos = map.fixedMessagePos.ToArray();
        }

        public int[] mapMatrix = null;
        public int[] dirMap = null;
        public int mapSize = 0;
        public int stairsBottomPosX = 0;
        public int stairsBottomPosY = 0;
        public int stairsBottomDir = 0;
        public int stairsTopPosX = 0;
        public int stairsTopPosY = 0;
        public int stairsTopDir = 0;
        public Pos[] tileOpenData = null;
        public Pos[] roomCenterPos = null;
        public Pos[] fixedMessagePos = null;
        public PosList[] randomMessagePos = null;
    }

    [System.Serializable]
    public class PosDirPair
    {
        public Pos pos;
        public int dir = 0;
    }

    [System.Serializable]
    public class PosList
    {
        public Pos[] pos;

        public PosList(List<Pos> posList)
        {
            pos = posList.ToArray();
        }

        public Pos this[int index] => pos[index];
    }

    [System.Serializable]
    public class RespawnData
    {
        public RespawnData(IEnumerable<EnemyData> enemyData, IEnumerable<ItemData> itemData)
        {
            this.enemyData = enemyData.ToArray();
            this.itemData = itemData.ToArray();
        }

        public EnemyData[] enemyData = null;
        public ItemData[] itemData = null;
    }

    [System.Serializable]
    public class EnemyData
    {
        public EnemyData(Pos pos, EnemyType type, IDirection dir, EnemyStatus.EnemyStoreData statusData)
        {
            this.pos = pos;
            enemyType = (int)type;
            this.dir = (int)dir.Enum;
            this.statusData = statusData;
        }

        public Pos pos;
        public int enemyType = 0;
        public int dir = 0;
        public EnemyStatus.EnemyStoreData statusData = null;
    }

    [System.Serializable]
    public class PlayerData
    {
        public PlayerData(Pos pos, IDirection dir, MobStatus.MobStoreData statusData)
        {
            this.pos = pos;
            this.dir = (int)dir.Enum;
            this.statusData = statusData;
        }

        public Pos pos;
        public int dir = 0;
        public MobStatus.MobStoreData statusData = null;
    }

    [System.Serializable]
    public class ItemData
    {
        public ItemData(Pos pos, ItemType type, int numOfItem)
        {
            this.pos = pos;
            itemType = (int)type;
            this.numOfItem = numOfItem;
        }

        public Pos pos;
        public int itemType = 0;
        public int numOfItem = 0;
    }

    [System.Serializable]
    public class InfoRecord : DataArray
    {
        public ulong moneyAmount = 0;
        public string causeOfDeath = "";
        public int floor = 1;
        public override object[] GetValues() => new object[] { };
    }

    protected List<DeadRecord> deadRecords = null;
    protected List<ClearRecord> clearRecords = null;
    protected InfoRecord infoRecord = null;
    protected SaveData saveData = null;

    protected MyAesGcm aesGcm;
    protected NonceStore nonceStore;

    protected ApplicationExitHandler exitHandler;

    protected override void Awake()
    {
        base.Awake();

        byte[] key = new byte[]{
            0x3C,0xD7,0x3C,0x3B,0x4E,0x41,0x1C,0xC8,
            0x08,0xA1,0xAA,0x93,0x36,0xB2,0x11,0xE6,
            0xFD,0xA7,0xB7,0x92,0x32,0x54,0x42,0x93,
            0xA1,0x0E,0xAE,0x80,0xB5,0xA4,0xC0,0x09
        };

        byte[] tagHashKey = new byte[] {
            0x4C, 0x32, 0x01, 0xF4, 0x46, 0x85, 0x07, 0xFC,
            0x0E, 0xAE, 0x80, 0xAA, 0x32, 0x89, 0xC0, 0xA1
        };

        nonceStore = new NonceStore(key, tagHashKey);
        aesGcm = new MyAesGcm(key, nonceStore);

        exitHandler = new ApplicationExitHandler(this);
    }

    public void SaveDeadRecords(IAttacker attacker, ulong moneyAmount, int currentFloor)
    {
        deadRecords = LoadDeadRecords();

        var causeOfDeath = attacker.Name + "にやられた";
        deadRecords.Add(new DeadRecord(moneyAmount, causeOfDeath, currentFloor));

        deadRecords = deadRecords.OrderByDescending(record => record.moneyAmount).Where((r, index) => index < 10).ToList();

        SaveEncryptedRecords(deadRecords, DEAD_RECORD_FILE_NAME);
    }

    public void SaveClearRecords(string title, ulong wagesAmount, int clearTimeSec, int defeatCount)
    {
        clearRecords = LoadClearRecords();

        clearRecords.Add(new ClearRecord(title, wagesAmount, clearTimeSec, defeatCount));

        clearRecords = clearRecords.OrderByDescending(record => record.wagesAmount).Where((r, index) => index < 10).ToList();

        SaveEncryptedRecords(clearRecords, CLEAR_RECORD_FILE_NAME);
    }

    protected void SaveEncryptedRecords<T>(List<T> records, string fileName) where T : DataArray
        => SaveEncryptedRecord(new RecordList<T>(records), fileName);

    public bool SaveCurrentGameData()
    {
        var gameInfo = GameInfo.Instance;

        saveData = new SaveData()
        {
            currentFloor = gameInfo.currentFloor,
            elapsedTimeSec = TimeManager.Instance.elapsedTimeSec + 1,
            playerData = PlayerInfo.Instance.ExportRespawnData(),
            mapData = gameInfo.ExportMapData(),
            respawnData = SpawnHandler.Instance.ExportRespawnData()
        };

        SaveEncryptedRecord(saveData, SAVE_DATA_FILE_NAME);
        return true;
    }

    protected void SaveEncryptedRecord<T>(T record, string fileName)
    {
        var encrypt = aesGcm.Encrypt(JsonUtility.ToJson(record));
        var nonceData = nonceStore.GetNanceData(encrypt.Key);

        SaveText(fileName, JsonUtility.ToJson(
            new RecordSet()
            {
                tag = encrypt.Key,
                hash = nonceData.Key,
                nonce = nonceData.Value,
                data = encrypt.Value
            }));
    }

    protected List<T> LoadRecords<T>(string fileName) where T : DataArray
    {
        return JsonUtility.FromJson<RecordList<T>>(LoadJsonData(fileName)).records;
    }

    public List<DeadRecord> LoadDeadRecords()
    {
        if (deadRecords != null) return deadRecords;

        try
        {
            deadRecords = LoadRecords<DeadRecord>(DEAD_RECORD_FILE_NAME);
        }
        catch (Exception e)
        {
            Debug.LogError("ファイルのロードに失敗: " + e.Message);
            deadRecords = new List<DeadRecord>();
        }

        return deadRecords;
    }

    public List<ClearRecord> LoadClearRecords()
    {
        if (clearRecords != null) return clearRecords;

        try
        {
            clearRecords = LoadRecords<ClearRecord>(CLEAR_RECORD_FILE_NAME);
        }
        catch (Exception e)
        {
            Debug.LogError("ファイルのロードに失敗: " + e.Message);
            clearRecords = new List<ClearRecord>();
        }

        return clearRecords;
    }
    public InfoRecord LoadInfoRecord()
    {
        if (infoRecord != null) return infoRecord;

        try
        {
            infoRecord = JsonUtility.FromJson<InfoRecord>(LoadJsonData(INFO_RECORD_FILE_NAME));
        }
        catch (Exception e)
        {
            Debug.LogError("loading is not implemented: " + e.Message);
            infoRecord = new InfoRecord();
        }

        return infoRecord;
    }

    public SaveData LoadGameData()
    {
        if (saveData != null) return saveData;

        try
        {
            saveData = JsonUtility.FromJson<SaveData>(LoadJsonData(SAVE_DATA_FILE_NAME));
            saveData.mapData = saveData.mapData.Select(data => data.mapSize == 0 ? null : data).ToArray();
        }
        catch (Exception e)
        {
            Debug.LogError("ファイルのロードに失敗: " + e.Message);
        }

        return saveData;
    }

    public bool ImportGameData()
    {
        var saveData = LoadGameData();

        if (saveData == null) return false;

        try
        {
            GameInfo.Instance.ImportGameData(saveData);
        }
        catch (Exception e)
        {
            Debug.LogError("データのインポートに失敗: " + e.Message);
            DeleteFile(SAVE_DATA_FILE_NAME);
            return false;
        }

        return true;
    }

    public void RespawnByGameData(WorldMap map, HidePlateHandler hidePlateHandler)
    {
        try
        {
            if (saveData == null) throw new Exception("データがロードされていません");
            TimeManager.Instance.AddTimeSec(saveData.elapsedTimeSec);
            SpawnHandler.Instance.ImportRespawnData(saveData.respawnData, map);
            PlayerInfo.Instance.ImportRespawnData(saveData.playerData);
            hidePlateHandler.Init();
        }
        catch (Exception e)
        {
            Debug.LogError("データのインポートに失敗: " + e.Message);
            DeleteFile(SAVE_DATA_FILE_NAME);
            Debug.Log(e.StackTrace);
            throw e;
        }
    }

    protected string LoadJsonData(string fileName)
    {
        RecordSet set = JsonUtility.FromJson<RecordSet>(LoadText(fileName));
        nonceStore.SetNanceData(set.hash, set.nonce);
        return aesGcm.Decrypt(set.data, set.tag);
    }

    public void SaveText(string fileName, string textToSave)
    {
        var combinedPath = Path.Combine(GetSecureDataPath(), fileName);
        using (var streamWriter = new StreamWriter(combinedPath))
        {
            streamWriter.WriteLine(textToSave);
        }
    }

    public string LoadText(string fileName)
    {
        var combinedPath = Path.Combine(GetSecureDataPath(), fileName);
        using (var streamReader = new StreamReader(combinedPath))
        {
            return streamReader.ReadToEnd();
        }
    }

    public void DeleteFile(string fileName)
    {
        File.Delete(Path.Combine(GetSecureDataPath(), fileName));
    }

    ///  <summary>
    /// Secure directory path for data save. Supports Android only for now.
    /// </summary>
    /// <returns>"/data/data/{ApplicationPackageName}/files" for example</returns>
    protected string GetSecureDataPath()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        using (var getFilesDir = currentActivity.Call<AndroidJavaObject>("getFilesDir"))
        {
            string secureDataPathForAndroid = getFilesDir.Call<string>("getCanonicalPath");
            return secureDataPathForAndroid;
        }
#else
        return Application.persistentDataPath;
#endif
    }

    public void EnableSave() => exitHandler.EnableSave();
    public void DisableSave() => exitHandler.DisableSave();


    protected class ApplicationExitHandler
    {
        private DataStoreAgent agent;
        private bool isSaveReserved = false;

        public ApplicationExitHandler(DataStoreAgent agent)
        {
            this.agent = agent;
        }

        public void EnableSave()
        {
            if (isSaveReserved) return;
            Application.wantsToQuit += agent.SaveCurrentGameData;
            isSaveReserved = true;
        }

        public void DisableSave()
        {
            if (!isSaveReserved) return;
            Application.wantsToQuit += agent.SaveCurrentGameData;
            isSaveReserved = false;
        }
    }
}