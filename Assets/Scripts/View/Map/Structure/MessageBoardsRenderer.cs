using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MessageBoardsRenderer : ObjectsRenderer<GameObject>
{
    private Dictionary<Terrain, GameObject> prefabMessageN = new Dictionary<Terrain, GameObject>();
    private FloorMessagesSource floorMessages;
    private Stack<int> randomIndices;

    public MessageBoardsRenderer(Transform parent) : base(parent)
    {
        prefabMessageN[Terrain.MessageWall] = prefabMessageN[Terrain.MessagePillar] = Resources.Load<GameObject>("Prefabs/Map/Parts/BoardN");
        prefabMessageN[Terrain.BloodMessageWall] = Resources.Load<GameObject>("Prefabs/Map/Parts/BloodMessageN");
        prefabMessageN[Terrain.BloodMessagePillar] = Resources.Load<GameObject>("Prefabs/Map/Parts/BloodMessagePillarN");
    }

    public override void SwitchWorldMap(WorldMap map)
    {
        this.map = map;
        floorMessages = ResourceLoader.Instance.floorMessagesData.Param(map.floor - 1);
        randomIndices = null;
    }

    public void SetMessage(Pos pos, IDirection dir, Terrain type)
    {
        PlacePrefab(pos, prefabMessageN[type], dir.Rotate);
        switch (type)
        {
            case Terrain.MessageWall:
            case Terrain.MessagePillar:
                SetMessageBoard(pos, dir);
                break;
            case Terrain.BloodMessageWall:
            case Terrain.BloodMessagePillar:
                SetBloodMessage(pos, dir);
                break;
        }
    }

    private void SetMessageBoard(Pos pos, IDirection dir)
    {
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

    private void SetBloodMessage(Pos pos, IDirection dir, bool isWall = true)
    {
        MessageWall tile = map.GetTile(pos) as MessageWall;
        tile.boardDir = dir;

        if (tile.Read == null)
        {
            int bloodIndex = map.bloodMessagePos.IndexOf(pos);
            if (bloodIndex != -1)
            {
                tile.data = floorMessages.bloodMessages[bloodIndex].Convert();
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
