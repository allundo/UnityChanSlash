using UnityEngine;
using UniRx;

public interface IReactor
{
    Vector3 position { get; }
    float OnDamage(float attack, IDirection dir, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None);
    void OnDie();
    void OnActive();
    void Destroy();
}

[RequireComponent(typeof(MobInput))]
[RequireComponent(typeof(MapUtil))]
public abstract class Reactor : MonoBehaviour, IReactor
{
    protected IStatus status;
    protected IMapUtil map;
    protected IInput input;
    protected IBodyEffect effect;
    protected Collider bodyCollider;

    public Vector3 position => transform.position;

    protected virtual void Awake()
    {
        status = GetComponent<MobStatus>();
        input = GetComponent<MobInput>();
        map = GetComponent<MapUtil>();
        bodyCollider = GetComponentInChildren<Collider>();
    }

    protected virtual void Start()
    {
        status.Life
            .SkipLatestValueOnSubscribe()
            .Subscribe(life => OnLifeChange(life))
            .AddTo(this);

        status.Active.Subscribe(_ => OnActive()).AddTo(this);
    }

    protected virtual void OnLifeChange(float life)
    {
        if (life <= 0.0f) input.InputDie();
    }

    public abstract float OnDamage(float attack, IDirection dir, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None);

    public virtual void OnDie()
    {
        effect.OnDie();
        map.ResetTile();
        bodyCollider.enabled = false;
    }

    public virtual void OnActive()
    {
        effect.OnActive();
        map.OnActive();
        input.OnActive();
        bodyCollider.enabled = true;
    }

    protected virtual void OnDead() => status.Inactivate();

    public virtual void Destroy()
    {
        // Stop all tweens before destroying
        input.ClearAll();
        effect.OnDestroy();

        bodyCollider.enabled = false;
        map.ResetTile();

        Destroy(gameObject);
    }
}
