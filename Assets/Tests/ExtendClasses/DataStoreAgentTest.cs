using UnityEngine;
using System;
using System.Collections.Generic;

public class DataStoreAgentTest : DataStoreAgent
{
    private Dictionary<string, string> storedData = new Dictionary<string, string>();
    private string[] fileNames;

    public void KeepSaveDataFiles()
    {
        HandleSaveDataFiles(name => storedData[name] = LoadText(name));
    }

    public void RestoreSaveDataFiles()
    {
        // Delete test data
        HandleSaveDataFiles(name => DeleteFile(name));

        // Restore data files
        storedData.ForEach(kv => SaveText(kv.Key, kv.Value));
    }

    public void SaveEncryptedRecordTest<T>(T record, string fileName) => SaveEncryptedRecord(record, fileName);

    private void HandleSaveDataFiles(Action<string> process)
    {
        foreach (var name in new string[] { DEAD_RECORD_FILE_NAME, CLEAR_RECORD_FILE_NAME, INFO_RECORD_FILE_NAME, SAVE_DATA_FILE_NAME, SETTING_DATA_FILE_NAME })
        {
            try
            {
                process(name);
            }
            catch (Exception e)
            {
                Debug.Log("failed to open file: " + e.Message);
            }
        }
    }
}
