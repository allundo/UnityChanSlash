using UnityEngine;
using System;
using System.Collections.Generic;

public class DataStoreAgentTest : DataStoreAgent
{
    private Dictionary<string, string> storedData = new Dictionary<string, string>();

    public void KeepSaveDataFiles()
    {
        foreach (var name in new string[] { DEAD_RECORD_FILE_NAME, CLEAR_RECORD_FILE_NAME, INFO_RECORD_FILE_NAME, SAVE_DATA_FILE_NAME })
        {
            try
            {
                var data = LoadText(name);
                storedData[name] = data;
            }
            catch (Exception e)
            {
                Debug.Log("failed to open file: " + e.Message);
            }
        }

    }

    public void RestoreSaveDataFiles()
    {
        // Restore data files
        storedData.ForEach(kv => SaveText(kv.Key, kv.Value));
    }

    public void SaveEncryptedRecordTest<T>(T record, string fileName) => SaveEncryptedRecord(record, fileName);
}
