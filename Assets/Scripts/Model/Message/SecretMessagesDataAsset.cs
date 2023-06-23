using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SecretMessagesDataAsset", menuName = "ScriptableObjects/CreateSecretMessagesDataAsset")]
public class SecretMessagesDataAsset : DataAsset<SecretMessageSource>
{
    protected SecretMessageData[] messageDataList = null;

    void Awake()
    {
        messageDataList = new SecretMessageData[setParams.Length];
        for (int i = 0; i < setParams.Length; i++)
        {
            messageDataList[i] = new SecretMessageData(setParams[i], i);
        }
    }

    public SecretMessageData Convert(int messageID)
    {
        if (messageID >= setParams.Length) throw new IndexOutOfRangeException("Secret message ID " + messageID + "is not found.");
        return messageDataList[messageID];
    }

    public SecretMessageData GetRandom(int floor, int secretLevel)
    {
        return messageDataList.Where(data => data.IsValid(floor, secretLevel)).GetRandom();
    }
}
