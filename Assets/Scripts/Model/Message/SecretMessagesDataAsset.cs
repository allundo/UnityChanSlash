using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SecretMessagesDataAsset", menuName = "ScriptableObjects/CreateSecretMessagesDataAsset")]
public class SecretMessagesDataAsset : DataAsset<SecretMessageSource>
{
    protected SecretMessageData[] messageDataList = null;

    void Awake()
    {
        messageDataList ??= LoadDataList();
    }

    protected SecretMessageData[] LoadDataList()
    {
        var list = new SecretMessageData[setParams.Length];
        for (int i = 0; i < setParams.Length; i++)
        {
            list[i] = new SecretMessageData(setParams[i], i);
        }
        return list;
    }

    public SecretMessageData Convert(int messageID)
    {
        messageDataList ??= LoadDataList();

        if (messageID >= setParams.Length) throw new IndexOutOfRangeException("Secret message ID " + messageID + "is not found.");
        return messageDataList[messageID];
    }

    public SecretMessageData[] GetFloorMessages(int floor, int secretLevel)
    {
        messageDataList ??= LoadDataList();
        return messageDataList.Where(data => data.IsValid(floor, secretLevel)).ToArray();
    }
}
