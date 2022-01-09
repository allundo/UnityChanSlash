using UnityEngine;

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
        FadeOutOnDead(0.5f);
    }
}
