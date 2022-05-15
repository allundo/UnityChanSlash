using UnityEngine;
using UniRx;

public interface IReactor
{
    Vector3 position { get; }
    float Damage(float attack, IDirection dir, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None);
    void OnDie();
    void Destroy();
}

[RequireComponent(typeof(Status))]
public abstract class Reactor : MonoBehaviour, IReactor
{
    protected IStatus status;
    protected Collider bodyCollider;

    public Vector3 position => transform.position;

    protected virtual void Awake()
    {
        status = GetComponent<Status>();
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

    protected abstract void OnLifeChange(float life);

    public abstract float Damage(float attack, IDirection dir, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None);

    public abstract void OnDie();

    protected abstract void OnActive();

    protected virtual void OnDead() => status.Inactivate();

    public abstract void Destroy();
}
