using UnityEngine;

[CreateAssetMenu(fileName = "BoardMessageData", menuName = "ScriptableObjects/CreateMessageSourceAsset")]
public class BoardMessageData : DataAsset<MessageSource>
{
    public MessageData Convert() => MessageData.Convert(setParams);
}
