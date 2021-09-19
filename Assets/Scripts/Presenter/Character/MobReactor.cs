using UnityEngine;
using UniRx;
using DG.Tweening;

[RequireComponent(typeof(MobStatus))]
[RequireComponent(typeof(MobEffect))]
[RequireComponent(typeof(MobCommander))]
public class MobReactor : SpawnObject<MobReactor>
{
    [SerializeField] protected PlayerLifeGauge lifeGauge = default;
    protected MobStatus status;
    protected MobEffect effect;
    protected MobCommander commander;

    // private ISubject<MobReactor> onDead = new Subject<MobReactor>();
    // public IObservable<MobReactor> OnDead => onDead;

    private static readonly Vector3 OUT_OF_SCREEN = new Vector3(-100.0f, 0.0f, -100.0f);

    protected virtual void Awake()
    {
        status = GetComponent<MobStatus>();
        effect = GetComponent<MobEffect>();
        commander = GetComponent<MobCommander>();
    }

    protected virtual void Start()
    {
        status.Life
            .SkipLatestValueOnSubscribe()
            .Subscribe(life => OnLifeChange(life))
            .AddTo(this);

        status.LifeMax
            .SkipLatestValueOnSubscribe()
            .Subscribe(lifeMax => OnLifeMaxChange(lifeMax))
            .AddTo(this);

        lifeGauge?.UpdateLifeText(status.Life.Value, status.LifeMax.Value);
    }

    protected void OnLifeChange(float life)
    {
        if (life <= 0.0f) OnDie();
        lifeGauge?.OnLifeChange(life, status.LifeMax.Value);
    }

    protected void OnLifeMaxChange(float lifeMax)
    {
        lifeGauge?.OnLifeChange(status.Life.Value, lifeMax);
    }

    public virtual void OnDamage(float attack, IDirection dir)
    {
        if (!status.IsAlive) return;

        float damage = CalcDamage(attack, dir);
        float damageRatio = Mathf.Clamp(damage / status.LifeMax.Value, 0.0f, 1.0f);

        effect.OnDamage(damageRatio);
        lifeGauge?.OnDamage(damageRatio);

        status.Damage(damage);
    }

    protected virtual float CalcDamage(float attack, IDirection dir)
    {
        return status.CalcAttack(attack, dir);
    }

    protected virtual void OnDie()
    {
        effect.OnDie();
        commander.SetDie();
    }

    public override MobReactor OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5f)
    {
        status.map.SetPosition(pos, dir);
        FadeInToActive(duration);
        return this;
    }

    public virtual void FadeInToActive(float duration = 0.5f)
    {
        Activate();
        effect.FadeInTween(duration).Play();
    }

    public virtual void FadeOutToDead(float duration = 0.5f)
    {
        effect.FadeOutTween(duration)
            .OnComplete(() => Dead())
            .Play();
    }

    public override void Activate()
    {
        status.Activate();
        commander.Activate();
    }

    public override void Inactivate()
    {
        status.Inactivate();
        commander.Inactivate();
    }

    protected virtual void Dead()
    {
        transform.position = OUT_OF_SCREEN;
        Inactivate();
        // onDead.OnNext(this);
    }
}