using UnityEngine;

public class DoorsRenderer : StructuresRenderer<DoorControl>
{
    private DoorControl prefabDoorV;
    private DoorHControl prefabDoorH;
    private LockedDoorControl prefabLockedDoorN;
    private ExitDoorControl prefabExitDoorN;

    public DoorsRenderer(Transform parent) : base(parent)
    {
        prefabDoorV = Resources.Load<DoorControl>("Prefabs/Map/DoorV");
        prefabDoorH = Resources.Load<DoorHControl>("Prefabs/Map/DoorH");
        prefabLockedDoorN = Resources.Load<LockedDoorControl>("Prefabs/Map/LockedDoorN");
        prefabExitDoorN = Resources.Load<ExitDoorControl>("Prefabs/Map/ExitDoorN");
    }

    public void SetDoorV(Pos pos) => SetDoor(pos, prefabDoorV);
    public void SetDoorH(Pos pos) => SetDoor(pos, prefabDoorH);

    private void SetDoor<T>(Pos pos, T doorPrefab) where T : DoorControl
    {
        InitDoorData(pos, PlacePrefab(pos, doorPrefab));
    }

    public void SetLockedDoor(Pos pos, IDirection dir)
    {
        InitDoorData(pos, PlacePrefab(pos, prefabLockedDoorN, dir).SetDir(dir), ItemType.TreasureKey);
    }

    public void SetExitDoor(Pos pos, IDirection dir)
    {
        InitDoorData(pos, PlacePrefab(pos, prefabExitDoorN, dir).SetDir(dir), ItemType.KeyBlade);
    }

    private void InitDoorData(Pos pos, DoorControl doorInstance, ItemType lockType = ItemType.Null)
    {
        var state = new DoorState(lockType);

        doorInstance.SetMaterials(floorMaterials.gate, floorMaterials.door).SetState(state);
        (map.GetTile(pos) as Door).state = state;
    }

    protected override void OnDestroyObject(DoorControl door) => door.KillTween();
    public void CompleteTween() => objectsPool.ForEach(door => door.CompleteTween());
}
