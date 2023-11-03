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

    protected virtual TControl PlacePrefab<TControl>(Pos pos, TControl prefab, IDirection dir = null) where TControl : T
    {
        var instance = Instantiate(prefab, pos, dir);
        objectsPool.Push(instance);
        return instance;
    }

    protected TPref Instantiate<TPref>(TPref prefab, Pos pos, IDirection dir = null) where TPref : UnityEngine.Object
        => Util.Instantiate(prefab, map.WorldPos(pos), dir != null ? dir.Rotate : Quaternion.identity, parentTransform);

    public virtual void DestroyObjects()
    {
        objectsPool.ForEach(obj => Object.Destroy(obj));
        objectsPool.Clear();
    }
}
