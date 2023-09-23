using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "FloorMessagesData", menuName = "ScriptableObjects/CreateFloorMessagesSourceAsset")]
public class FloorMessagesData : DataAsset<FloorMessagesSource>
{
    public static readonly int MAX_ELEMENTS = 10;

    void Awake()
    {
        setParams.ForEach(src =>
        {
            if (
                src.bloodMessages.Length > MAX_ELEMENTS
                || src.fixedMessages.Length > MAX_ELEMENTS
                || src.randomMessages.Length > MAX_ELEMENTS
                || src.secretMessages.Length > MAX_ELEMENTS
            )
            {
                throw new IndexOutOfRangeException($"Floor message data is out of max elements: MAX_ELEMENTS = {MAX_ELEMENTS}");
            }
        });
    }

    public MessageData[][] GetRandomMessages() => setParams.Select(floorSrc => floorSrc.randomMessages.Select(randomSrc => randomSrc.Convert()).ToArray()).ToArray();

    public SecretMessageData[][] GetSecretMessages(int floor, int secretLevel)
    {
        var data = new SecretMessageData[floor][];
        for (int i = 0; i < floor; ++i)
        {
            var floorData = new List<SecretMessageData>();
            var secret = setParams[i].secretMessages;

            for (int j = 0; j < secret.Length; ++j)
            {
                // secret level を超えるメッセージは除外
                if (secret[j].secretLevel > secretLevel) continue;

                floorData.Add(new SecretMessageData(secret[j], i * MAX_ELEMENTS + j));
            }

            data[i] = floorData.ToArray();
        }

        return data;
    }

    public MessageData GetDiary(int messageID)
    {
        int floor = messageID / MAX_ELEMENTS;
        int subID = messageID % MAX_ELEMENTS;

        return new MessageData(setParams[floor].secretMessages[subID].data.Convert().Source[0]);
    }

}
