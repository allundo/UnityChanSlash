using UnityEngine;
using UniRx;

[RequireComponent(typeof(MobStatus))]
[RequireComponent(typeof(MobVisual))]
[RequireComponent(typeof(MobCommander))]
public class MobReactor : MonoBehaviour
{
    protected MobStatus status;
    protected MobVisual visual;
    protected MobCommander commander;

    [SerializeField] private AudioSource dieSound = null;

    // public ISubject<Unit> Inactivator { get; protected set; } = new Subject<Unit>();

    protected virtual void Awake()
    {
        status = GetComponent<MobStatus>();
        visual = GetComponent<MobVisual>();
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
        visual.DamageFlash(damage, status.LifeMax);
    }

    protected void OnDie()
    {
        if (dieSound != null)
        {
            dieSound.Play();
        }

        commander.SetDie();
    }

    protected virtual void OnActivate()
    {
        Activate();
    }

    protected virtual void OnInactivate()
    {
        Inactivate();
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