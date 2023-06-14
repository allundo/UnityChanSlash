using UnityEngine;

[RequireComponent(typeof(DarkHoundInput))]
public class DarkHoundReactor : BulletReactor
{
    protected Transform targetTf = null;
    public float TargetAngle => targetTf == null ? 0f : Vector3.SignedAngle(transform.forward, targetTf.position - transform.position, Vector3.up);
    protected ILauncher healSpiritLauncher;
    protected ILauncher darkSpiritLauncher;

    protected override void Awake()
    {
        base.Awake();
        healSpiritLauncher = new Launcher(status, MagicType.HealSpirit);
        darkSpiritLauncher = new Launcher(status, MagicType.DarkSpirit);
    }

    private void OnTriggerEnter(Collider other)
    {
        MobReactor targetMob = other.GetComponent<MobReactor>();
        if (targetMob != null)
        {
            targetTf = targetMob.transform;
            bodyCollider.enabled = false;
        }
    }

    protected override void Hit(float drain)
    {
        if (!status.IsAlive) return;

        var lifeMax = status.LifeMax.Value;

        var launcher = drain < 0 ? darkSpiritLauncher : healSpiritLauncher;
        var spiritsPower = Mathf.Abs(drain) * 0.5f;

        // Spirits refers to shooter(DarkHound's) status to calculate attack or heal power.
        (status as IMagicStatus).SetAttack(Mathf.Max(spiritsPower * 0.2f, 0.05f));

        for (float power = 0f; power < spiritsPower; power += status.attack) launcher.Fire();

        effect.OnHit();
        Die();
    }

    protected override void OnActive()
    {
        base.OnActive();
        targetTf = null;
        bodyCollider.enabled = true;
    }

    public override void OnDie()
    {
        bodyCollider.enabled = false;
        base.OnDie();
    }
}
