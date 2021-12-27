using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider))]
public class MobAttack : Attack
{
    [SerializeField] protected int startFrame = 0;
    [SerializeField] protected int finishFrame = 0;
    [SerializeField] protected int speed = 1;
    [SerializeField] protected int frameRate = 30;


    private float FrameToSec(int frame)
    {
        return (float)frame / (float)frameRate / (float)speed;
    }

    public override Tween AttackSequence(float attackDuration)
    {
        return DOTween.Sequence()
            .Join(DOVirtual.DelayedCall(FrameToSec(startFrame), OnHitStart))
            .Join(DOVirtual.DelayedCall(FrameToSec(finishFrame), OnHitFinished))
            .SetUpdate(false);
    }
}
