using UnityEngine;
using System;
using System.Linq;

public class DataStoreAgentTest : DataStoreAgent
{
    private string[] tempFiles;
    private string[] fileNames;

    public void KeepSaveDataFiles()
    {
        fileNames = new string[] {
            DEAD_RECORD_FILE_NAME,
            CLEAR_RECORD_FILE_NAME,
            INFO_RECORD_FILE_NAME,
            SAVE_DATA_FILE_NAME
        };

        try
        {
            tempFiles = fileNames.Select(name => LoadText(name)).ToArray();
        }
        catch (Exception e)
        {
            Debug.Log("failed to open file: " + e.Message);
        }
    }

    public void RestoreSaveDataFiles()
    {
        // Restore data files
        for (int i = 0; i < fileNames.Length; i++)
        {
            SaveText(fileNames[i], tempFiles[i]);
        }
    }

    public void SaveEncryptedRecordTest<T>(T record, string fileName) => SaveEncryptedRecord(record, fileName);
}
