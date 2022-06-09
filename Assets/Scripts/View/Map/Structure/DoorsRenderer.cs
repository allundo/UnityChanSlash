using UnityEngine;
using System.Collections.Generic;

public class DoorsRenderer
{
    private WorldMap map;
    private FloorMaterialsData floorMaterialsData;

    private DoorControl prefabDoorV;
    private DoorHControl prefabDoorH;
    private ExitDoorControl prefabExitDoorN;

    private Transform parentTransform;

    private Stack<DoorControl> doorsPool = new Stack<DoorControl>();

    private Material floorMatDoor;
    private Material floorMatGate;

    public DoorsRenderer(Transform parent)
    {
        this.parentTransform = parent;
        this.floorMaterialsData = ResourceLoader.Instance.floorMaterialsData;

        prefabDoorV = Resources.Load<DoorControl>("Prefabs/Map/DoorV");
        prefabDoorH = Resources.Load<DoorHControl>("Prefabs/Map/DoorH");
        prefabExitDoorN = Resources.Load<ExitDoorControl>("Prefabs/Map/ExitDoorN");
    }

    public virtual void LoadFloorMaterials(WorldMap map)
    {
        this.map = map;
        var floorMaterials = floorMaterialsData.Param(map.floor - 1);
        floorMatDoor = floorMaterials.door;
        floorMatGate = floorMaterials.gate;
    }

    public void SetDoorV(Pos pos) => SetDoor(pos, prefabDoorV);
    public void SetDoorH(Pos pos) => SetDoor(pos, prefabDoorH);

    private void SetDoor<T>(Pos pos, T doorPrefab) where T : DoorControl
    {
        InitDoorData(pos, PlacePrefab(pos, doorPrefab));
    }

    public void SetExitDoor(Pos pos, IDirection dir)
    {
        InitDoorData(pos, PlacePrefab(pos, prefabExitDoorN, dir.Rotate).SetDir(dir), ItemType.KeyBlade);
    }

    private T PlacePrefab<T>(Pos pos, T prefab) where T : DoorControl
        => PlacePrefab(pos, prefab, Quaternion.identity);

    private T PlacePrefab<T>(Pos pos, T prefab, Quaternion rotation) where T : DoorControl
    {
        var instance = Util.Instantiate(prefab, map.WorldPos(pos), rotation, parentTransform);
        doorsPool.Push(instance);
        return instance;
    }

    private void InitDoorData(Pos pos, DoorControl doorInstance, ItemType lockType = ItemType.Null)
    {
        var state = new DoorState(lockType);

        doorInstance.SetMaterials(floorMatGate, floorMatDoor).SetState(state);
        (map.GetTile(pos) as Door).state = state;
    }

    public virtual void DestroyObjects()
    {
        doorsPool.ForEach(door =>
        {
            door.KillTween();
            Object.Destroy(door);
        });
    }
}