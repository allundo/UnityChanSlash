using UnityEngine;

public class BoxesRenderer : StructuresRenderer<BoxControl>
{
    private BoxControl prefabBoxN;

    public BoxesRenderer(Transform parent) : base(parent)
    {
        prefabBoxN = Resources.Load<BoxControl>("Prefabs/Map/TreasureBoxN");
    }

    public override void SwitchWorldMap(WorldMap map)
    {
        this.map = map;
    }

    public void SetBox(Pos pos, IDirection dir) => PlacePrefab(pos, prefabBoxN, dir).SetTileState((map.GetTile(pos) as Box));


    protected override void OnDestroyObject(BoxControl box) => box.KillTween();
    public void CompleteTween() => objectsPool.ForEach(box => box.CompleteTween());
}
