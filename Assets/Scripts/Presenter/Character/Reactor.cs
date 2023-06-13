using UnityEngine;
using UniRx;

public interface IReactor
{
    void OnDie();
    void Destroy();
}

[RequireComponent(typeof(Status))]
public abstract class Reactor : MonoBehaviour, IReactor
{
    protected IStatus status;
    protected Collider bodyCollider;

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

    public abstract void OnDie();

    protected abstract void OnActive();

    protected virtual void OnDead() => status.Inactivate();

    public abstract void Destroy();
}
