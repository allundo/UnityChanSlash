using UnityEngine;
using UniRx;
using DG.Tweening;

[RequireComponent(typeof(Renderer))]
public class Item : SpawnObject<Item>
{
    private Renderer meshRenderer;

    public ItemInfo itemInfo { get; protected set; }

    protected virtual void Awake()
    {
        meshRenderer = GetComponent<Renderer>();
    }

    void Start()
    {
        PlayerInfo.Instance.DirObservable
            .Subscribe(dir => SetDir(dir))
            .AddTo(this);
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

    private void SetDir(IDirection dir)
    {
        transform.DORotate(dir.Angle, 0.04f).SetEase(Ease.InQuad).Play();
    }

    public Item SetItemInfo(ItemInfo info)
    {
        itemInfo = info;
        meshRenderer.material = info.material;
        gameObject.name = info.type.ToString();
        return this;
    }

    public void SetDisplay(bool isShown = true)
    {
        meshRenderer.enabled = isShown;
    }
}
