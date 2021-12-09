using UnityEngine;

[RequireComponent(typeof(BulletStatus))]
[RequireComponent(typeof(BulletInput))]
[RequireComponent(typeof(BulletEffect))]
public class BulletReactor : MobReactor
{
    public override void Activate()
    {
        status.OnActive();
        effect.OnActive();
        input.OnActive();
    }

    public override void OnDie()
    {
        effect.OnDie();
        FadeOutOnDead(0.5f);
    }
}
