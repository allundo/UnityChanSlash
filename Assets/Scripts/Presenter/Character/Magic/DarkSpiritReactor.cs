using UnityEngine;

public class DarkSpiritReactor : HealSpiritReactor
{
    protected override void OnTriggerEnter(Collider other)
    {
        MobReactor targetMob = other.GetComponent<MobReactor>();
        if (bulletStatus.shotBy.gameObject != targetMob?.gameObject) return;

        targetMob.Damage(status.attack, Direction.Convert(transform.forward), AttackType.Dark, AttackAttr.Dark);
        Damage(20f, null);
        bodyCollider.enabled = false;
    }
}