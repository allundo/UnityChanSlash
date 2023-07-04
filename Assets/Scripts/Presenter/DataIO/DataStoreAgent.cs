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
    public readonly string SETTING_DATA_FILE_NAME = "setting_data.dat";

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
    public class SaveData
    {
        public int currentFloor = 0;
        public int elapsedTimeSec = 0;
        public PlayerData playerData = null;
        public ItemInfo[] inventoryItems = null;
        public int currentEvent = -1;
        public EventData[] eventData = null;
        public MapData[] mapData = null;
        public bool isExitDoorLocked = true;
        public RespawnData[] respawnData = null;
    }

    [System.Serializable]
    public class MapData : IStairsData
    {
        public MapData(WorldMap map)
        {
            var converter = map.dirMapHandler;
            mapMatrix = converter.ConvertMapData();
            dirMap = converter.ConvertDirData();
            mapSize = converter.width;

            var tileState = map.tileStateHandler.ExportTileStateData();
            tileOpenData = tileState.open.ToArray();
            tileBrokenData = tileState.broken.ToArray();
            messageReadData = tileState.read.ToArray();

            tileDiscoveredData = map.miniMapData.ExportTileDiscoveredData();
            roomCenterPos = map.roomCenterPos.ToArray();

            var mesData = map.messagePosData;
            fixedMessagePos = mesData.fixedMessagePos.ToArray();
            bloodMessagePos = mesData.bloodMessagePos.ToArray();
            randomMessagePos = mesData.ExportRandomMessagePos();
            secretMessagePos = mesData.ExportSecretMessagePos();

            stairsData = map.stairsMapData.ExportValues;
        }

        public int[] mapMatrix = null;
        public int[] dirMap = null;
        public int mapSize = 0;
        public Pos[] tileOpenData = null;
        public Pos[] tileBrokenData = null;
        public Pos[] messageReadData = null;
        public bool[] tileDiscoveredData = null;
        public Pos[] roomCenterPos = null;
        public Pos[] fixedMessagePos = null;
        public PosList[] randomMessagePos = null;
        public PosList[] secretMessagePos = null;
        public Pos[] bloodMessagePos = null;
        public Pos[] stairsData = new Pos[3];

        public Pos upStairs { get => stairsData[0]; private set => stairsData[0] = value; }
        public Pos downStairs { get => stairsData[1]; private set => stairsData[1] = value; }
        public Pos exitDoor { get => stairsData[2]; private set => stairsData[2] = value; }
    }

    [System.Serializable]
    public class EventData
    {
        public EventData(int[] eventList)
        {
            this.eventList = eventList;
        }
        public int[] eventList;
    }

    [System.Serializable]
    public class PosDirPair
    {
        public PosDirPair(KeyValuePair<Pos, IDirection> pair) : this(pair.Key, pair.Value) { }

        public PosDirPair(Pos pos, IDirection dir)
        {
            this.pos = pos;
            this.dir = dir?.Int ?? 0;
        }

        public KeyValuePair<Pos, IDirection> Convert() => new KeyValuePair<Pos, IDirection>(pos, Direction.Convert(dir));

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
    public class EnemyData : MobData
    {
        public EnemyData(Pos pos, IEnemyStatus status) : base(pos, status)
        {
            type = (int)status.type;
            isTamed = status.isTamed;
            if (status is IUndeadStatus) curse = (status as IUndeadStatus).curse;
        }

        [SerializeField] private int type = 0;
        public EnemyType enemyType
        {
            get { return Util.ConvertTo<EnemyType>(type); }
            set { type = (int)value; }
        }

        public bool isTamed = false;
        public float curse = 0f;
        public EnemyStoreData StoreData() => new EnemyStoreData(level, life, isTamed, curse);
    }

    [System.Serializable]
    public class PlayerData : MobData
    {
        public PlayerData(Pos pos, PlayerStatus status, ExitState state)
        : this(pos, status.dir, status.Life.Value, status.level, status.Exp, status.isHidden, status.UpdateIcingFrames(), state, status.counter, status.selector.type)
        { }

        /// <summary>
        /// For testing
        /// </summary>
        public PlayerData(Pos pos, IDirection dir, float life, int level, float exp, bool isHidden, float icingFrames, ExitState state, PlayerCounter counter, LevelGainType type)
        : base(pos, dir, life, level, isHidden, icingFrames)
        {
            this.state = (int)state;
            this.exp = exp;
            this.counter = counter;
            this.type = (int)type;
        }

        [SerializeField] private int state;

        public ExitState exitState
        {
            get { return Util.ConvertTo<ExitState>(state); }
            set { state = (int)value; }
        }

        public float exp = 0f;
        public PlayerCounter counter;

        [SerializeField] private int type;

        public LevelGainType levelGainType
        {
            get { return Util.ConvertTo<LevelGainType>(type); }
            set { type = (int)value; }
        }
    }

    [System.Serializable]
    public class MobData
    {
        public MobData(Pos pos, IMobStatus status) : this(pos, status.dir, status.Life.Value, status.level, status.isHidden, status.UpdateIcingFrames())
        { }

        /// <summary>
        /// For testing
        /// </summary>
        public MobData(Pos pos, IDirection dir, float life, int level, bool isHidden, float icingFrames)
        {
            posDir = new PosDirPair(pos, dir);
            this.life = life;
            this.level = level;
            this.isHidden = isHidden;
            this.icingFrames = icingFrames;
        }

        [SerializeField] private PosDirPair posDir;

        public KeyValuePair<Pos, IDirection> kvPosDir
        {
            get { return posDir.Convert(); }
            set
            {
                pos = value.Key;
                dir = value.Value;
            }
        }

        public Pos pos
        {
            get { return posDir.pos; }
            set { posDir.pos = value; }
        }

        public IDirection dir
        {
            get { return Direction.Convert(posDir.dir); }
            set { posDir.dir = value.Int; }
        }

        public float life = 0f;
        public bool isHidden = false;
        public float icingFrames = 0f;
        public int level = 0;
    }

    [System.Serializable]
    public class ItemData
    {
        public ItemData(Pos pos, ItemType type, int numOfItem)
        {
            this.pos = pos;
            itemInfo = new ItemInfo(type, numOfItem);
        }

        public Pos pos;
        [SerializeField] private ItemInfo itemInfo;

        public ItemType itemType
        {
            get { return itemInfo.itemType; }
            set { itemInfo.itemType = value; }
        }
        public int numOfItem
        {
            get { return itemInfo.numOfItem; }
            set { itemInfo.numOfItem = value; }
        }
    }

    [System.Serializable]
    public class ItemInfo
    {
        public ItemInfo(ItemType type, int numOfItem)
        {
            this.type = (int)type;
            this.numOfItem = numOfItem;
        }

        [SerializeField] private int type = 0;
        public ItemType itemType
        {
            get { return Util.ConvertTo<ItemType>(type); }
            set { type = (int)value; }
        }

        public int numOfItem = 0;
    }

    [System.Serializable]
    public class InfoRecord : DataArray
    {
        public int minSteps = 999999999;
        public float maxMap = 0f;
        public int playTime = 0;
        public int deadCount = 0;
        public int clearCount = 0;
        public int[] readMessageIDs = new int[0];
        public int secretLevel = 0;

        [SerializeField] private string[] titles = new string[0];
        public bool HasTitle(string title) => titles.Contains(title);

        public string[] TitleList(string unknown = null)
            => PlayerCounter.Titles
                .Select(title => HasTitle(title) ? title : unknown)
                .ToArray();

        private void GetTitle(string title)
        {
            var set = titles.ToHashSet();
            set.Add(title);
            titles = set.ToArray();
        }

        public void UpdateClearRecord(GameInfo info)
        {
            if (minSteps > info.steps) minSteps = info.steps;
            if (maxMap < info.mapComp) maxMap = info.mapComp;
            GetTitle(info.title);
            clearCount++;
        }

        public void UpdateAccumulation(GameInfo info)
        {
            playTime += info.endTimeSec - info.storedTimeSec;
            readMessageIDs = info.readIDs.ToArray();
            secretLevel = info.secretLevel;
            if (!PlayerInfo.Instance.IsPlayerAlive) deadCount++;
        }

        public override object[] GetValues() => new object[] { minSteps, maxMap, playTime, clearCount, deadCount };
    }

    [System.Serializable]
    public class SettingData
    {
        public SettingData()
        {
            alpha = new float[Util.Count<UIType>()].Select(_ => 1f).ToArray();
        }

        public SettingData(params float[] alpha)
        {
            this.alpha = alpha;
            this.alpha[0] = 1f;
        }

        [SerializeField] private float[] alpha;

        private static string[] labels = new string[]
        {
            "Default UI",
            "敵体力ゲージ",
            "攻撃種別アイコン",
            "攻撃種別領域",
            "移動・ガードボタン",
            "操作ボタン",
        };

        public float this[UIType type]
        {
            get { return alpha[(int)type]; }
            set { if (type != UIType.None) alpha[(int)type] = value; }
        }

        public static string Label(UIType type) => labels[(int)type];
    }

    protected List<DeadRecord> deadRecords = null;
    protected List<ClearRecord> clearRecords = null;
    protected InfoRecord infoRecord = null;
    protected SaveData saveData = null;
    protected SettingData settingData = null;

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

    /// <summary>
    /// Update dead records by new record and save it into save data file.
    /// </summary>
    /// <param name="newRecord">new dead record data</param>
    /// <returns>rank of the new dead record, 0 if the rank is out of display.</returns>
    public int SaveDeadRecords(DataStoreAgent.DeadRecord newRecord)
    {
        // Delete save data
        DisableSave();
        DeleteSaveDataFile();

        deadRecords = LoadDeadRecords();

        deadRecords.Add(newRecord);

        deadRecords = deadRecords
            .OrderByDescending(record => record.moneyAmount)
            .ThenBy(record => record.floor).Where((r, index) => index < 10).ToList();

        var rank = deadRecords.IndexOf(newRecord) + 1;

        SaveEncryptedRecords(deadRecords, DEAD_RECORD_FILE_NAME);
        return rank;
    }

    public int SaveClearRecords(DataStoreAgent.ClearRecord newRecord)
    {
        // Delete save data
        DisableSave();
        DeleteSaveDataFile();

        clearRecords = LoadClearRecords();

        clearRecords.Add(newRecord);

        clearRecords = clearRecords.OrderByDescending(record => record.wagesAmount).Where((r, index) => index < 10).ToList();

        var rank = clearRecords.IndexOf(newRecord) + 1;

        SaveEncryptedRecords(clearRecords, CLEAR_RECORD_FILE_NAME);
        return rank;
    }

    public void SaveInfoRecord(GameInfo info)
    {
        DisableSave();

        infoRecord = LoadInfoRecord();

        if (info.clearRecord != null) infoRecord.UpdateClearRecord(info);

        infoRecord.UpdateAccumulation(info);

        SaveEncryptedRecord(infoRecord, INFO_RECORD_FILE_NAME);
    }

    protected void SaveEncryptedRecords<T>(List<T> records, string fileName) where T : DataArray
        => SaveEncryptedRecord(new RecordList<T>(records), fileName);

    public void SaveOnMovingFloor(bool isDownStair)
    {
        var gameInfo = GameInfo.Instance;
        var gameManager = GameManager.Instance;
        var map = gameManager.worldMap;

        var stairsEnterPos = map.StairsEnter(isDownStair).Key;
        int nextFloor = gameInfo.currentFloor + (isDownStair ? 1 : -1);

#if UNITY_EDITOR
        // Never save if played game from MainScene directly.
        if (GameInfo.Instance.isScenePlayedByEditor)
        {
            SpawnHandler.Instance.SaveEnemyRespawnData(stairsEnterPos);
            map.tileStateHandler.StoreTileStateData();
            return;
        }
#endif
        var data = PlayerInfo.Instance.ExportRespawnData();
        var nextMap = gameInfo.Map(nextFloor, false);
        if (nextMap != null) data.kvPosDir = nextMap.StairsExit(isDownStair);

        saveData = new SaveData()
        {
            currentFloor = nextFloor,
            elapsedTimeSec = TimeManager.Instance.elapsedTimeSec + 1,
            playerData = data,
            inventoryItems = ItemInventory.Instance.ExportInventoryItems(),
            mapData = gameInfo.ExportMapData(),
            isExitDoorLocked = ITileStateData.isExitDoorLocked,
            currentEvent = gameManager.GetCurrentEvent(),
            eventData = gameManager.ExportEventData(),
            respawnData = SpawnHandler.Instance.ExportRespawnData(stairsEnterPos)
        };

        SaveEncryptedRecord(saveData, SAVE_DATA_FILE_NAME);
    }

    public bool SaveCurrentGameData()
    {
#if UNITY_EDITOR
        // Never save if played game from MainScene directly.
        if (GameInfo.Instance.isScenePlayedByEditor) return true;
#endif

        var gameInfo = GameInfo.Instance;
        var gameManager = GameManager.Instance;
        var playerInfo = PlayerInfo.Instance;

        saveData = new SaveData()
        {
            currentFloor = gameInfo.currentFloor,
            elapsedTimeSec = TimeManager.Instance.elapsedTimeSec + 1,
            playerData = playerInfo.ExportRespawnData(),
            inventoryItems = ItemInventory.Instance.ExportInventoryItems(),
            mapData = gameInfo.ExportMapData(),
            isExitDoorLocked = ITileStateData.isExitDoorLocked,
            currentEvent = gameManager.GetCurrentEvent(),
            eventData = gameManager.ExportEventData(),
            respawnData = SpawnHandler.Instance.ExportRespawnData(playerInfo.Pos)
        };

        gameInfo.endTimeSec = saveData.elapsedTimeSec;
        SaveInfoRecord(gameInfo);
        gameInfo.storedTimeSec = saveData.elapsedTimeSec;

        SaveEncryptedRecord(saveData, SAVE_DATA_FILE_NAME);
        return true;
    }

    public void SaveSettings(SettingData data)
    {
        settingData = data;
        SaveEncryptedRecord(settingData, SETTING_DATA_FILE_NAME);
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
            Debug.LogError($"ファイルのロードに失敗: {e.Message} ...新しくファイルを作成します");
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
            Debug.LogError($"ファイルのロードに失敗: {e.Message} ...新しくファイルを作成します");
            clearRecords = new List<ClearRecord>();
        }

        return clearRecords;
    }
    public InfoRecord LoadInfoRecord()
    {
        try
        {
            infoRecord = infoRecord ?? JsonUtility.FromJson<InfoRecord>(LoadJsonData(INFO_RECORD_FILE_NAME));
        }
        catch (Exception e)
        {
            Debug.LogError($"ファイルのロードに失敗: {e.Message} ...新しくファイルを作成します");
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

        var infoRecord = LoadInfoRecord();

        try
        {
            GameInfo.Instance.ImportGameData(saveData, infoRecord);
        }
        catch (Exception e)
        {
            Debug.LogError($"データのインポートに失敗: {e.Message} \n{e.StackTrace}");
            DeleteSaveDataFile();
            return false;
        }

        return true;
    }

    public void RespawnByGameData(WorldMap map)
    {
        try
        {
            if (saveData == null) throw new Exception("データがロードされていません");

            TimeManager.Instance.AddTimeSec(saveData.elapsedTimeSec);
            GameInfo.Instance.storedTimeSec = saveData.elapsedTimeSec;
            SpawnHandler.Instance.ImportRespawnData(saveData.respawnData, map);
            PlayerInfo.Instance.ImportRespawnData(saveData.playerData, map);
            ItemInventory.Instance.ImportInventoryItems(saveData.inventoryItems);
            GameManager.Instance.ImportRespawnData(saveData);
        }
        catch (Exception e)
        {
            Debug.LogError("データのインポートに失敗: " + e.Message);
            DeleteSaveDataFile();
            Debug.Log(e.StackTrace);
            throw e;
        }
    }

    public void RestorePlayerStatus()
    {
        PlayerInfo.Instance.RestorePlayerStatus(saveData.playerData);
    }

    public SettingData LoadSettingData()
    {
        try
        {
            settingData = settingData ?? JsonUtility.FromJson<SettingData>(LoadJsonData(SETTING_DATA_FILE_NAME));
        }
        catch (Exception e)
        {
            Debug.LogError("設定ファイルのロードに失敗: " + e.Message);
            SaveSettings(new SettingData());
        }

        return settingData;
    }

    public float GetSettingData(UIType type)
    {
        try
        {
            return LoadSettingData()[type];
        }
        catch (Exception e)
        {
            Debug.LogError("設定ファイルが不正です: " + e.Message);
            DeleteSettingDataFile();
            settingData = new SettingData();
            return settingData[type];
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

    public void DeleteSaveDataFile()
    {
        DeleteFile(SAVE_DATA_FILE_NAME);
        saveData = null;
    }

    public void DeleteSettingDataFile()
    {
        DeleteFile(SETTING_DATA_FILE_NAME);
        settingData = null;
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

#if !UNITY_EDITOR && UNITY_ANDROID
    private bool isAutoSaveEnabled = false;
#endif

    public void EnableSave()
    {
        exitHandler.EnableSave();

#if !UNITY_EDITOR && UNITY_ANDROID
        isAutoSaveEnabled = true;
#endif
    }

    public void DisableSave()
    {
        exitHandler.DisableSave();

#if !UNITY_EDITOR && UNITY_ANDROID
        isAutoSaveEnabled = false;
#endif
    }

#if !UNITY_EDITOR && UNITY_ANDROID
    private void OnApplicationPause(bool isPauseOn)
    {
        if(!(isPauseOn && isAutoSaveEnabled)) return;
        SaveCurrentGameData();
    }
#endif

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
            Application.wantsToQuit -= agent.SaveCurrentGameData;
            isSaveReserved = false;
        }
    }
}