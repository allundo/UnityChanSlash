using UnityEngine;

[RequireComponent(typeof(BulletStatus))]
[RequireComponent(typeof(DarkHoundInput))]
[RequireComponent(typeof(BulletEffect))]
public class DarkHoundReactor : BulletReactor
{
    protected Transform targetTf = null;
    public float TargetAngle => targetTf == null ? 0f : Vector3.SignedAngle(transform.forward, targetTf.position - transform.position, Vector3.up);
    protected ILauncher healSpiritLauncher;

    protected override void Awake()
    {
        base.Awake();
        healSpiritLauncher = new Launcher(status, BulletType.HealSpirit);
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

        (status as IBulletStatus).SetAttack(Mathf.Max(drain * 0.1f, 0.05f));

        while (drain > 0.0001f)
        {
            healSpiritLauncher.Fire();
            drain -= status.attack * 2f;
        }

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
