using UnityEngine;
using UniRx;
using DG.Tweening;

[RequireComponent(typeof(BulletStatus))]
[RequireComponent(typeof(HealSpritEffect))]
public class HealSpritReactor : MonoBehaviour, IBulletReactor
{
    protected BulletStatus status;
    protected IBodyEffect effect;
    protected Collider bodyCollider;
    protected bool isTweening = false;
    protected Tween emittingTween = null;

    public Vector3 position => transform.position;

    protected virtual void Awake()
    {
        status = GetComponent<BulletStatus>();
        effect = GetComponent<HealSpritEffect>();
        bodyCollider = GetComponent<Collider>();
    }

    protected virtual void Start()
    {
        status.Life
            .SkipLatestValueOnSubscribe()
            .Subscribe(life => OnLifeChange(life))
            .AddTo(this);

        status.Active.Subscribe(_ => OnActive()).AddTo(this);
    }

    protected virtual void Update()
    {
        if (isTweening) return;
        transform.position += (status.shotBy.Position - transform.position).normalized * Time.deltaTime * 2.5f;
        ReduceHP(Time.deltaTime);
    }

    protected void OnLifeChange(float life)
    {
        if (life <= 0.0f) OnDie();
    }

    private void OnTriggerEnter(Collider other)
    {
        MobReactor targetMob = other.GetComponent<MobReactor>();
        if (status.shotBy.gameObject != targetMob?.gameObject) return;

        targetMob.Heal(status.attack);
        Damage(20f, null);
        bodyCollider.enabled = false;
    }

    public void ReduceHP(float reduction = 1f)
    {
        if (status.IsAlive) status.LifeChange(-reduction);
    }

    public float Damage(float attack, IDirection dir, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None)
    {
        if (!status.IsAlive) return 0f;

        status.LifeChange(-attack);
        effect.OnDamage(attack, type, attr);

        return 10f;
    }

    public void OnDie()
    {
        bodyCollider.enabled = false;
        effect.OnDie();
        effect.Disappear(OnDead);
        isTweening = true;
    }

    public virtual void OnActive()
    {
        effect.OnActive();
        bodyCollider.enabled = true;
        isTweening = true;
        emittingTween = transform.DOMove(UnityEngine.Random.onUnitSphere * 2f, 0.5f)
            .SetRelative(true)
            .OnComplete(() => isTweening = false)
            .Play();
    }

    protected void OnDead() => status.Inactivate();

    public void Destroy()
    {
        // Stop all tweens before destroying
        effect.OnDestroy();
        bodyCollider.enabled = false;

        Destroy(gameObject);
    }
}