using UniRx;
using UnityEngine;

public class EnemyStatus : MobStatus
{
    protected EnemyParam enemyParam;

    public EnemyType type => enemyParam.type;
    public Vector3 enemyCorePos => enemyParam.enemyCore + transform.position;

    public override MobStatus InitParam(MobParam param, float life = 0f)
    {
        enemyParam = param as EnemyParam;
        return base.InitParam(param, life);
    }

    protected override void OnActive()
    {
        // Don't reset status on activation
        // Set status by InitParam()

        activeSubject.OnNext(Unit.Default);
    }
}
