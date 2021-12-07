using UnityEngine;
using DG.Tweening;

public class Fire : MonoBehaviour, IAttack
{
    protected MobStatus status;

    protected virtual void Awake()
    {
        status = GetComponentInParent<MobStatus>();
    }

    public virtual Tween AttackSequence(float attackDuration)
    {
        return
            DOVirtual.DelayedCall(
                attackDuration * 0.3f,
                () => GameManager.Instance.FireBall(status.transform.position, status.dir, status.Attack),
                false
            );
    }
}
