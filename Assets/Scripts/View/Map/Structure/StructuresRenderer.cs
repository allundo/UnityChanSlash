using UnityEngine;
using System.Collections.Generic;

public interface IStructuresRenderer
{
    void SwitchWorldMap(WorldMap map);
    void DestroyObjects();
}

public abstract class StructuresRenderer<T> : IStructuresRenderer
    where T : UnityEngine.Object
{
    protected WorldMap map;
    protected FloorMaterialsSource floorMaterials;

    private Transform parentTransform;

    private Stack<T> structuresPool = new Stack<T>();

    public StructuresRenderer(Transform parent)
    {
        this.parentTransform = parent;
    }

    public virtual void SwitchWorldMap(WorldMap map)
    {
        this.map = map;
        floorMaterials = ResourceLoader.Instance.floorMaterialsData.Param(map.floor - 1);
    }

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
