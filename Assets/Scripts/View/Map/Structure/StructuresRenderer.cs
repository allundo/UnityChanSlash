using UnityEngine;
using System.Collections.Generic;

public abstract class StructuresRenderer<T>
    where T : MonoBehaviour
{
    protected WorldMap map;
    protected FloorMaterialsData floorMaterialsData;

    private Transform parentTransform;

    private Stack<T> structuresPool = new Stack<T>();

    public StructuresRenderer(Transform parent)
    {
        this.parentTransform = parent;
        this.floorMaterialsData = ResourceLoader.Instance.floorMaterialsData;
    }

    public abstract void LoadFloorMaterials(WorldMap map);

    protected TControl PlacePrefab<TControl>(Pos pos, TControl prefab) where TControl : T
        => PlacePrefab(pos, prefab, Quaternion.identity);

    protected TControl PlacePrefab<TControl>(Pos pos, TControl prefab, Quaternion rotation) where TControl : T
    {
        var instance = Util.Instantiate(prefab, map.WorldPos(pos), rotation, parentTransform);
        structuresPool.Push(instance);
        return instance;
    }

    public void DestroyObjects()
    {
        structuresPool.ForEach(structure =>
        {
            OnDestroyObject(structure);
            Object.Destroy(structure);
        });
    }

    protected virtual void OnDestroyObject(T structure) { }
}
