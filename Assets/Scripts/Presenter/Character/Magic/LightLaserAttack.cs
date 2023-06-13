using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class LightLaserAttack : BulletAttack
{
    public void SetColliderLength()
    {
        float length = (status as ILaserStatus).length * Constants.TILE_UNIT;

        var laserCollider = attackCollider as BoxCollider;

        laserCollider.center = new Vector3(0f, 1f, (length + 1.0f) * 0.5f);
        laserCollider.size = new Vector3(2f, 2f, length * SizeRatio);
    }

    protected virtual float SizeRatio => 1f;

    protected override IReactor OnHitAttack(Collider collider)
    {
        IReactor targetMob = collider.GetComponent<MobReactor>();

        if (null == targetMob) return null;

        var damage = targetMob.Damage(status.attack * attackMultiplier, Direction.Convert(transform.forward), attackType, attackAttr);
        return targetMob;
    }
}
