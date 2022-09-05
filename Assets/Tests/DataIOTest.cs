using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public class DataIOTest
{
    private DataStoreAgentTest dataStoreAgent;
    private string[] tempFiles;
    private string[] fileNames;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        dataStoreAgent = UnityEngine.Object.Instantiate(Resources.Load<DataStoreAgentTest>("Prefabs/System/DataStoreAgentTest"), Vector3.zero, Quaternion.identity); ;

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
}
