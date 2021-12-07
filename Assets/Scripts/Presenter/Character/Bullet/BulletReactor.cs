using UnityEngine;

[RequireComponent(typeof(BulletStatus))]
[RequireComponent(typeof(BulletInput))]
[RequireComponent(typeof(BulletEffect))]
public class BulletReactor : MobReactor
{
    public override MobReactor OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5f)
    {
        status.SetPosition(pos, dir, false);
        Activate();
        return this;
    }
}
