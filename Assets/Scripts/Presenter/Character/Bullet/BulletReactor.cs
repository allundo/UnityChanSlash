using UnityEngine;

[RequireComponent(typeof(BulletStatus))]
[RequireComponent(typeof(BulletInput))]
[RequireComponent(typeof(BulletEffect))]
public class BulletReactor : MobReactor
{
    protected override void OnActive()
    {
        base.OnActive();
        (input as BulletInput).InputFire();
    }
}
