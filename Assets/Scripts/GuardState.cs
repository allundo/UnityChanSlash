using UnityEngine;

public class GuardState : MonoBehaviour
{
    private ShieldCommander commander;
    private ShieldAnimator anim;
    private MapUtil map;

    void Start()
    {
        commander = GetComponent<ShieldCommander>();
        map = commander.map;
        anim = commander.anim as ShieldAnimator;
    }

    public bool IsAutoGuard { get; protected set; } = false;
    protected bool IsManualGuard = false;

    protected bool IsGuardOn => IsManualGuard || IsAutoGuard;

    protected int shieldCount = 0;
    [SerializeField] protected int SHIELD_READY = 10;
    public bool IsShieldReady => shieldCount == SHIELD_READY;

    public bool IsShieldOn(Direction attackDir) => commander.IsIdling && IsShieldReady && map.dir.IsInverse(attackDir);

    public void SetShield() { anim.shield.Fire(); }

    public virtual void SetEnemyDetected(bool isDetected)
    {
        IsAutoGuard = isDetected;
        anim.guard.Bool = IsGuardOn;
    }

    public virtual void SetManualGuard(bool isGuard)
    {
        IsManualGuard = isGuard;
        anim.guard.Bool = IsGuardOn;
    }

    void Update()
    {
        if (IsGuardOn)
        {
            if (shieldCount < SHIELD_READY) shieldCount++;
            return;
        }

        shieldCount = 0;
    }
}