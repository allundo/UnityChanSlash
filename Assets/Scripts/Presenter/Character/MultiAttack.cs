using UnityEngine;
using UniRx;
using System;
using System.Linq;
using DG.Tweening;

public class MultiAttack : AttackBehaviour, IAttackHitDetect
{
    [SerializeField] protected MobAttackFX[] attacks = default;

    protected Sequence attackSequence;

    public IObservable<IReactor> Hit => Observable.Merge(attacks.Select(atk => atk.Hit));

    protected virtual void Start()
    {
        Hit.Subscribe(_ => attackSequence.Complete(true)).AddTo(this);
    }

    public override Sequence AttackSequence(float attackDuration)
    {
        attackSequence?.Complete(true);
        attackSequence = DOTween.Sequence();

        attacks.ForEach(atk => attackSequence.Join(atk.AttackSequence(attackDuration)));
        return attackSequence.SetUpdate(false);
    }
}
