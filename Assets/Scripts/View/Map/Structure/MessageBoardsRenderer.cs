using UnityEngine;
using System.Collections.Generic;

public class MessageBoardsRenderer : ObjectsRenderer<GameObject>
{
    private Dictionary<Terrain, GameObject> prefabMessageN = new Dictionary<Terrain, GameObject>();
    private FloorMessagesSource floorMessages;

    public MessageBoardsRenderer(Transform parent) : base(parent)
    {
        prefabMessageN[Terrain.MessageWall] = prefabMessageN[Terrain.MessagePillar] = Resources.Load<GameObject>("Prefabs/Map/Parts/BoardN");
        prefabMessageN[Terrain.BloodMessageWall] = Resources.Load<GameObject>("Prefabs/Map/Parts/BloodMessageN");
        prefabMessageN[Terrain.BloodMessagePillar] = Resources.Load<GameObject>("Prefabs/Map/Parts/BloodMessagePillarN");
        prefabMessageN[Terrain.PictureWall] = Resources.Load<GameObject>("Prefabs/Map/Parts/PictureBoardN");
    }

    public override void SwitchWorldMap(WorldMap map)
    {
        this.map = map;
        floorMessages = ResourceLoader.Instance.floorMessagesData.Param(map.floor - 1);
    }

    public void SetMessage(Pos pos, IDirection dir, Terrain type)
    {
        PlacePrefab(pos, prefabMessageN[type], dir);
    }
}
