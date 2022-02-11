using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(BulletStatus))]
[RequireComponent(typeof(BulletInput))]
[RequireComponent(typeof(BulletEffect))]
public class BulletReactor : MobReactor
{
    public override void OnActive()
    {
        effect.OnActive();
        // Need to set MapUtil.onTilePos before input moving Command
        map.OnActive();
        input.OnActive();
    }

    public override void OnDie()
    {
        effect.OnDie();
        FadeOutToDead(0.5f);
    }

    public override void FadeOutToDead(float duration = 0.5f)
    {
        fadeOut = DOTween.Sequence()
            .Join(effect.FadeOutTween(duration))
            .AppendInterval(1.5f)
            .AppendCallback(OnDead)
            .Play();
    }
}
