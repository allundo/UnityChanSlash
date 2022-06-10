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

    public void SetBox(Pos pos, IDirection dir)
    {
        var state = new BoxState();
        var box = PlacePrefab(pos, prefabBoxN, dir.Rotate).SetState(state) as BoxControl;
        (map.GetTile(pos) as Box).state = state;
    }

    protected override void OnDestroyObject(BoxControl box) => box.KillTween();
    public void CompleteTween() => objectsPool.ForEach(box => box.CompleteTween());
}
