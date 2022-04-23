using UniRx;
using UnityEngine;

public interface IEnemyStatus : IMobStatus
{
    EnemyType type { get; }
}

public class EnemyStatus : MobStatus, IEnemyStatus
{
    protected EnemyParam enemyParam;

    public EnemyType type => enemyParam.type;
    public override Vector3 corePos => enemyParam.enemyCore + transform.position;

    public override IStatus InitParam(Param param, float life = 0f)
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
