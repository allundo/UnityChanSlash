using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Renderer))]
public class Item : SpawnObject<Item>
{
    private static readonly Dictionary<IDirection, Quaternion> angles =
        new Dictionary<IDirection, Quaternion>()
        {
            {Direction.north, Quaternion.identity},
            {Direction.east, Quaternion.Euler(0, 90, 0)},
            {Direction.south, Quaternion.Euler(0, 180, 0)},
            {Direction.west, Quaternion.Euler(0, -90, 0)},
        };

    private Renderer meshRenderer;

    public Material material => meshRenderer.material;

    public ItemInfo itemInfo { get; protected set; }

    public enum ItemTypeEnum
    {
    }

    protected virtual void Awake()
    {
        meshRenderer = GetComponent<Renderer>();
    }

    public override Item OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5F)
    {
        SetPos(pos);
        SetDir(dir);
        Activate();
        return this;
    }

    public Item SetPos(Vector3 pos)
    {
        transform.position = pos;
        return this;
    }

    public Item SetDir(IDirection dir)
    {
        transform.rotation = angles[dir];
        return this;
    }

    public Item SetItemInfo(ItemInfo info)
    {
        this.itemInfo = info;
        return this;
    }

    public Item SetMaterial(Material material)
    {
        meshRenderer.material = material;
        return this;
    }
}
