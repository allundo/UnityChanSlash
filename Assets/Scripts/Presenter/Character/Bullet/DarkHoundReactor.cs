using UnityEngine;

[RequireComponent(typeof(BulletStatus))]
[RequireComponent(typeof(DarkHoundInput))]
[RequireComponent(typeof(BulletEffect))]
public class DarkHoundReactor : BulletReactor
{
    public IStatus shotBy { protected get; set; } = null;
    protected Transform targetTf = null;
    public float TargetAngle => targetTf == null ? 0f : Vector3.SignedAngle(transform.forward, (targetTf.position - transform.position), Vector3.up);

    private void OnTriggerEnter(Collider other)
    {
        MobReactor targetMob = other.GetComponent<MobReactor>();
        if (targetMob != null)
        {
            targetTf = targetMob.transform;
            bodyCollider.enabled = false;
        }
    }

    public override void OnActive()
    {
        effect.OnActive();
        // Need to set MapUtil.onTilePos before input moving Command
        map.OnActive();
        input.OnActive();
        bodyCollider.enabled = true;
    }

    public override float OnDamage(float attack, IDirection dir, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None)
    {
        if (!status.IsAlive) return 0f;

        status.Damage(20f);
        effect.OnDamage(1f, type, attr);

        return 10f;
    }
    public override void OnDie()
    {
        bodyCollider.enabled = false;
        base.OnDie();
    }
}
