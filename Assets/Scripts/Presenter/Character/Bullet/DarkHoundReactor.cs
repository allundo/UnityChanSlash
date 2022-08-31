using UnityEngine;

[RequireComponent(typeof(BulletStatus))]
[RequireComponent(typeof(DarkHoundInput))]
[RequireComponent(typeof(BulletEffect))]
public class DarkHoundReactor : BulletReactor
{
    protected Transform targetTf = null;
    public float TargetAngle => targetTf == null ? 0f : Vector3.SignedAngle(transform.forward, targetTf.position - transform.position, Vector3.up);
    protected ILauncher healSpiritLauncher;
    protected ILauncher darkSpiritLauncher;

    protected override void Awake()
    {
        base.Awake();
        healSpiritLauncher = new Launcher(status, BulletType.HealSpirit);
        darkSpiritLauncher = new Launcher(status, BulletType.DarkSpirit);
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

    protected override void OnActive()
    {
        effect.OnActive();
        // Need to set MapUtil.onTilePos before input moving Command
        map.OnActive();
        input.OnActive();
        bodyCollider.enabled = true;
    }

    public override float Damage(float drain, IDirection dir, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None)
    {
        if (!status.IsAlive) return 0f;

        var lifeMax = status.LifeMax.Value;

        var launcher = drain < 0 ? darkSpiritLauncher : healSpiritLauncher;
        var spiritsPower = Mathf.Abs(drain) * 0.5f;

        // Spirits refers to shooter(DarkHound's) status to calculate attack or heal power.
        // TODO: Make sure not to refer to DarkHound's status after its destroying. e.g. Moving floor
        (status as IBulletStatus).SetAttack(Mathf.Max(spiritsPower * 0.2f, 0.05f));

        for (float power = 0f; power < spiritsPower; power += status.attack) launcher.Fire();

        effect.OnDamage(lifeMax, type, attr);
        status.LifeChange(-lifeMax);

        return lifeMax;
    }

    public override void OnDie()
    {
        bodyCollider.enabled = false;
        base.OnDie();
    }
}
