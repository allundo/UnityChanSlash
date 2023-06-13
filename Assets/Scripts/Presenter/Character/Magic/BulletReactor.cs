using UnityEngine;
using System;
using UniRx;

[RequireComponent(typeof(BulletEffect))]
public class BulletReactor : MagicReactor
{
    protected IDisposable hitDisposable;

    protected override void OnActive()
    {
        hitDisposable = (attack as IBulletAttack)
            .Hit
            .Subscribe(damage => Hit(damage))
            .AddTo(this);

        base.OnActive();
    }

    protected virtual void Hit(float damage)
    {
        if (!status.IsAlive) return;

        Die();
        effect.OnHit();
    }

    public override void OnDie()
    {
        hitDisposable?.Dispose();
        base.OnDie();
    }
}
