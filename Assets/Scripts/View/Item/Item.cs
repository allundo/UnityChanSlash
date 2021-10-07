using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Renderer))]
public class Item : SpawnObject<Item>
{
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
        transform.DORotate(dir.Angle, 0.04f).SetEase(Ease.InQuad).Play();
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
