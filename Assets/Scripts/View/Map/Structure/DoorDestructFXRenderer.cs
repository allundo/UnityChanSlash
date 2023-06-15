using UnityEngine;
using System.Linq;

public class DoorDestructFXRenderer : PooledStructuresRenderer<DoorDestructFX>
{
    public DoorDestructFX templateFX;

    public DoorDestructFXRenderer(Transform parent) : base(parent)
    {
        templateFX = Util.Instantiate(Resources.Load<DoorDestructFX>("Prefabs/Effect/DoorDestructFX"), parent);
        templateFX.Inactivate();
    }

    public override void SwitchWorldMap(WorldMap map)
    {
        base.SwitchWorldMap(map);
        templateFX.SetMaterial(floorMaterials.door);
        objectsPool.Push(templateFX);
    }

    public override void DestroyObjects()
    {
        objectsPool.ForEach(obj =>
        {
            obj.CompleteTween();
            if (obj != templateFX) Object.Destroy(obj.gameObject);
        });
        objectsPool.Clear();
    }

    public DoorDestructFX Spawn(Vector3 pos, IDirection dir, float duration = 3f)
    {
        return GetInstance(templateFX).OnSpawn(pos, dir, duration);
    }
}

public class PooledStructuresRenderer<T> : StructuresRenderer<T>
    where T : MonoBehaviour, ISpawnObject<T>
{
    public PooledStructuresRenderer(Transform parent) : base(parent) { }

    protected override TControl PlacePrefab<TControl>(Pos pos, TControl prefab, IDirection dir = null)
    {
        return GetInstance(prefab).OnSpawn(map.WorldPos(pos), dir) as TControl;
    }

    protected T GetInstance(T prefab)
    {
        var instance = objectsPool.FirstOrDefault(obj => !obj.gameObject.activeSelf);
        if (instance != null) return instance;

        objectsPool.Push(Util.Instantiate(prefab, parentTransform, true));
        return objectsPool.Peek();
    }
}