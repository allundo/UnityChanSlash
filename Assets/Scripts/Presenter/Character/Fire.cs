using UnityEngine;
using DG.Tweening;

public class Fire : MonoBehaviour, IAttack
{
    protected MobStatus status;
    protected FireBallGenerator fireBallGenerator;

    protected virtual void Awake()
    {
        status = GetComponent<MobStatus>();
        fireBallGenerator = GameManager.Instance.GetFireBallGenerator;
    }

    public virtual Tween AttackSequence(float attackDuration)
    {
        return
            DOVirtual.DelayedCall(
                attackDuration * 0.3f,
                () => fireBallGenerator.Fire(status.transform.position, status.dir, status.Attack),
                false
            );
    }
}
