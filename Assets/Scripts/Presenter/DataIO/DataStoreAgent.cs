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
        public string title = "なし";
        public ulong wagesAmount = 0;
        public ulong clearTimeSec = 0;
        public int defeatCount = 0;

        public override object[] GetValues() => new object[] { title, wagesAmount, clearTimeSec, defeatCount };
    }

    [System.Serializable]
    public class SaveData : DataArray
    {
        public ulong moneyAmount = 0;
        public string causeOfDeath = "";
        public int floor = 1;
        public override object[] GetValues() => new object[] { };
    }

    [System.Serializable]
    public class InfoRecord : DataArray
    {
        public ulong moneyAmount = 0;
        public string causeOfDeath = "";
        public int floor = 1;
        public override object[] GetValues() => new object[] { };
    }

    private List<DeadRecord> deadRecords = null;
    private List<ClearRecord> clearRecords = null;
    private InfoRecord infoRecord = null;
    private SaveData saveData = null;

    private MyAesGcm aesGcm;
    private NonceStore nonceStore;

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
    }

    public void SaveDeadRecords(IAttacker attacker, ulong moneyAmount, int currentFloor)
    {
        deadRecords = LoadDeadRecords();

        var causeOfDeath = attacker.Name + "にやられた";
        deadRecords.Add(new DeadRecord(moneyAmount, causeOfDeath, currentFloor));

        deadRecords = deadRecords.OrderByDescending(record => record.moneyAmount).Where((r, index) => index < 10).ToList();

        SaveEncryptedRecords(deadRecords, DEAD_RECORD_FILE_NAME);
    }

    private void SaveEncryptedRecords<T>(List<T> records, string fileName) where T : DataArray
    {
        var encrypt = aesGcm.Encrypt(JsonUtility.ToJson(new RecordList<T>(records)));
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

    private List<T> LoadRecords<T>(string fileName) where T : DataArray
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
        }
        catch (Exception e)
        {
            Debug.LogError("loading is not implemented: " + e.Message);
            saveData = new SaveData();
        }

        return saveData;
    }

    private string LoadJsonData(string fileName)
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
    private string GetSecureDataPath()
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
}