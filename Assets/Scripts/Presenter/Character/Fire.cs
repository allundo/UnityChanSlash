using UnityEngine;
using DG.Tweening;

public class Fire : MonoBehaviour, IAttack
{
    [SerializeField] protected BulletType type;
    protected BulletGenerator bulletGenerator;
    protected IStatus status;

    protected virtual void Awake()
    {
        status = GetComponent<MobStatus>();
        bulletGenerator = GameManager.Instance.GetBulletGenerator(type);
    }

    public virtual Tween AttackSequence(float attackDuration)
    {
        return
            DOVirtual.DelayedCall(
                attackDuration * 0.3f,
                () => bulletGenerator.Fire(status.Position, status.dir, status.Attack),
                false
            );
    }
}
