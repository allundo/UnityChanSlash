using UnityEngine;

public class StructuresRenderer<T> : ObjectsRenderer<T>
    where T : MonoBehaviour
{
    protected FloorMaterialsSource floorMaterials;

    public StructuresRenderer(Transform parent) : base(parent) { }

    public override void SwitchWorldMap(WorldMap map)
    {
        this.map = map;
        floorMaterials = ResourceLoader.Instance.floorMaterialsData.Param(map.floor - 1);
    }

    public override void DestroyObjects()
    {
        objectsPool.ForEach(structure =>
        {
            OnDestroyObject(structure);
            Object.Destroy(structure.gameObject); // !! Make sure to Destroy a `GameObject`, not a `component`
        });
        objectsPool.Clear();
    }

    protected virtual void OnDestroyObject(T structure) { }
}
