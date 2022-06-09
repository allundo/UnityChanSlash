using UnityEngine;

public class MessageBoardsRenderer : StructuresRenderer<GameObject>
{
    private GameObject prefabMessageBoardN;
    private FloorMessagesSource floorMessages;

    private MessageData[] FixedMessage(int index) => floorMessages.fixedMessages[index].Convert();
    private MessageData[] RandomMessage() => floorMessages.randomMessages[Random.Range(0, floorMessages.randomMessages.Length)].Convert();


    public MessageBoardsRenderer(Transform parent) : base(parent)
    {
        prefabMessageBoardN = Resources.Load<GameObject>("Prefabs/Map/Parts/BoardN");
    }

    public override void SwitchWorldMap(WorldMap map)
    {
        this.map = map;
        floorMessages = ResourceLoader.Instance.floorMessagesData.Param(map.floor - 1);
    }

    public void SetMessageBoard(Pos pos, IDirection dir)
    {
        PlacePrefab(pos, prefabMessageBoardN, dir.Rotate);
        MessageWall tile = map.GetTile(pos) as MessageWall;
        tile.boardDir = dir;

        if (tile.Read == null)
        {
            int fixedIndex = map.fixedMessagePos.IndexOf(pos);
            tile.data = fixedIndex != -1 ? FixedMessage(fixedIndex) : RandomMessage();
        }
    }
}