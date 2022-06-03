
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "BoardMessageData", menuName = "ScriptableObjects/CreateMessageSourceAsset")]
public class BoardMessageData : DataAsset<MessageSource>
{
    public MessageData[] Convert() => setParams.Select(source => source.Convert()).ToArray();
}
