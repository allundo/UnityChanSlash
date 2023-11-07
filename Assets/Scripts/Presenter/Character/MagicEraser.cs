using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public sealed class MagicEraser : MobAttack
{
    protected override IMobReactor OnHitAttack(Collider collider)
    {
        var target = collider.GetComponent<MagicReactor>();
        target?.Die();
        return null;
    }
}
