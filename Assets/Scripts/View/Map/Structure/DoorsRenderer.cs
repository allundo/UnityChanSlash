using UnityEngine;

public class DoorsRenderer : StructuresRenderer<DoorControl>
{
    private DoorControl prefabDoorV;
    private DoorHControl prefabDoorH;
    private ExitDoorControl prefabExitDoorN;

    private Material floorMatDoor;
    private Material floorMatGate;

    public DoorsRenderer(Transform parent) : base(parent)
    {
        prefabDoorV = Resources.Load<DoorControl>("Prefabs/Map/DoorV");
        prefabDoorH = Resources.Load<DoorHControl>("Prefabs/Map/DoorH");
        prefabExitDoorN = Resources.Load<ExitDoorControl>("Prefabs/Map/ExitDoorN");
    }

    public override void LoadFloorMaterials(WorldMap map)
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

    private void InitDoorData(Pos pos, DoorControl doorInstance, ItemType lockType = ItemType.Null)
    {
        var state = new DoorState(lockType);

        doorInstance.SetMaterials(floorMatGate, floorMatDoor).SetState(state);
        (map.GetTile(pos) as Door).state = state;
    }

    protected override void OnDestroyObject(DoorControl door) => door.KillTween();
}