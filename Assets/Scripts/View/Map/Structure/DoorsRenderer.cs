using UnityEngine;

public class DoorsRenderer : StructuresRenderer<DoorControl>
{
    private DoorControl prefabDoorV;
    private DoorHControl prefabDoorH;
    private SealableDoorControl prefabSealableDoorN;
    private LockedDoorControl prefabLockedDoorN;
    private ExitDoorControl prefabExitDoorN;

    public DoorsRenderer(Transform parent) : base(parent)
    {
        prefabDoorV = Resources.Load<DoorControl>("Prefabs/Map/DoorV");
        prefabDoorH = Resources.Load<DoorHControl>("Prefabs/Map/DoorH");
        prefabSealableDoorN = Resources.Load<SealableDoorControl>("Prefabs/Map/SealableDoorN");
        prefabLockedDoorN = Resources.Load<LockedDoorControl>("Prefabs/Map/LockedDoorN");
        prefabExitDoorN = Resources.Load<ExitDoorControl>("Prefabs/Map/ExitDoorN");
    }

    public void SetDoorV(Pos pos) => SetDoor(pos, prefabDoorV);
    public void SetDoorH(Pos pos) => SetDoor(pos, prefabDoorH);

    private void SetDoor<T>(Pos pos, T doorPrefab) where T : DoorControl
    {
        PlaceDoor(pos, doorPrefab);
    }
    public void SetSealableDoor(Pos pos, IDirection dir)
    {
        PlaceDoor(pos, prefabSealableDoorN, dir).SetDir(dir);
    }

    public void SetLockedDoor(Pos pos, IDirection dir)
    {
        PlaceDoor(pos, prefabLockedDoorN, dir).SetDir(dir);
    }

    public void SetExitDoor(Pos pos, IDirection dir)
    {
        PlaceDoor(pos, prefabExitDoorN, dir).SetDir(dir);
    }

    private T PlaceDoor<T>(Pos pos, T doorPrefab, IDirection dir = null) where T : DoorControl
    {
        return PlacePrefab(pos, doorPrefab, dir)
            .SetMaterials(floorMaterials.gate, floorMaterials.door)
            .SetTileState(map.GetTile(pos) as Door) as T;
    }

    protected override void OnDestroyObject(DoorControl door) => door.KillTween();
    public void CompleteTween() => objectsPool.ForEach(door => door.CompleteTween());
}
