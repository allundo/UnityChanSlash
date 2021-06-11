using UnityEngine;
using UniRx;
using DG.Tweening;

[RequireComponent(typeof(MobStatus))]
[RequireComponent(typeof(MobEffect))]
[RequireComponent(typeof(MobCommander))]
public class MobReactor : MonoBehaviour
{
    protected MobStatus status;
    protected MobEffect effect;
    protected MobCommander commander;

    // public ISubject<Unit> Inactivator { get; protected set; } = new Subject<Unit>();

    protected virtual void Awake()
    {
        status = GetComponent<MobStatus>();
        effect = GetComponent<MobEffect>();
        commander = GetComponent<MobCommander>();
    }

    protected virtual void Start()
    {
        status.Life.Subscribe(life => OnLifeChange(life)).AddTo(this);
        // Call Activate() directly for now.
        // Inactivator.Subscribe(_ => OnInactivate()).AddTo(this);
    }

    protected void OnLifeChange(float life)
    {
        if (life <= 0.0f) OnDie();
    }

    public virtual void OnDamage(float attack, Direction dir)
    {
        if (!status.IsAlive) return;

        float damage = status.CalcAttack(attack, dir);

        effect.OnDamage(damage, status.LifeMax);

        status.Damage(damage);
    }

    protected virtual void OnDie()
    {
        effect.OnDie();
        commander.SetDie();
    }

    public virtual void FadeInToActive(float duration = 0.5f)
    {
        Activate();
        effect.FadeInTween(duration).Play();
    }

    public virtual void FadeOutToInactive(float duration = 0.5f)
    {
        effect.FadeOutTween(duration)
            .OnComplete(() => Inactivate())
            .Play();
    }

    public void Activate()
    {
        status.Activate();
        commander.Activate();
    }

    public void Inactivate()
    {
        status.Inactivate();
        commander.Inactivate();
    }
}