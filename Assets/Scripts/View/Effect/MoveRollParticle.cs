using UnityEngine;
using DG.Tweening;

public class MoveRollParticle : RollParticle
{
    [SerializeField] private Vector3 moveVec = Vector3.forward;
    [SerializeField] private Ease moveEase = Ease.Linear;

    protected override void Awake()
    {
        base.Awake();
        JoinTween(transform.DOLocalMove(transform.localPosition + moveVec, duration).SetEase(moveEase));
    }
}