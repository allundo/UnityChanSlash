using UnityEngine;
using UniRx;

public class MobReactor : MonoBehaviour
{
    protected MobStatus status;
    protected MobCommander commander;

    [SerializeField] private AudioSource dieSound = null;

    // public ISubject<Unit> Inactivator { get; protected set; } = new Subject<Unit>();

    protected virtual void Awake()
    {
        status = GetComponent<MobStatus>();
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