using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MessageBoardsRenderer : ObjectsRenderer<GameObject>
{
    private GameObject prefabMessageBoardN;
    private FloorMessagesSource floorMessages;
    private Stack<int> randomIndices = null;

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
            if (fixedIndex != -1)
            {
                tile.data = floorMessages.fixedMessages[fixedIndex].Convert();
            }
            else
            {
                int randomIndex;
                if (!map.randomMessagePos.TryGetValue(pos, out randomIndex))
                {
                    randomIndex = GetRandomIndex();
                    map.randomMessagePos[pos] = randomIndex;
                }

                tile.data = floorMessages.randomMessages[randomIndex].Convert();
            }
        }
    }

    private int GetRandomIndex()
    {
        if (randomIndices == null || randomIndices.Count == 0)
        {
            randomIndices = Enumerable.Range(0, floorMessages.randomMessages.Length).Shuffle().ToStack();
        }

        return randomIndices.Pop();
    }
}
