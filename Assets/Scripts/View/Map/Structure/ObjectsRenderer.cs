using UnityEngine;
using System.Collections.Generic;

public interface IObjectsRenderer
{
    void SwitchWorldMap(WorldMap map);
    void DestroyObjects();
}

public abstract class ObjectsRenderer<T> : IObjectsRenderer
    where T : UnityEngine.Object
{
    protected WorldMap map;

    protected Transform parentTransform;

    protected Stack<T> objectsPool = new Stack<T>();

    public ObjectsRenderer(Transform parent)
    {
        this.parentTransform = parent;
    }

    public virtual void SwitchWorldMap(WorldMap map)
    {
        this.map = map;
    }

    protected TControl PlacePrefab<TControl>(Pos pos, TControl prefab) where TControl : T
        => PlacePrefab(pos, prefab, Quaternion.identity);

    protected TControl PlacePrefab<TControl>(Pos pos, TControl prefab, Quaternion rotation) where TControl : T
    {
        var instance = Util.Instantiate(prefab, map.WorldPos(pos), rotation, parentTransform);
        objectsPool.Push(instance);
        return instance;
    }

    public virtual void DestroyObjects()
    {
        objectsPool.ForEach(obj => Object.Destroy(obj));
        objectsPool.Clear();
    }
}
