using UnityEngine;
using UniRx;

public class BulletStatus : MobStatus
{
    public override float CalcAttack(float attack, IDirection attackDir) => attack;

    public override MobStatus OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5f)
    {
        Activate();
        map.SetPosition(pos, dir, false);
        return this;
    }

    public override void Activate()
    {
        if (isActive) return;

        isActive = true;
        gameObject.SetActive(true);
        ResetStatus();
        onActive.OnNext(Unit.Default);
    }
}
