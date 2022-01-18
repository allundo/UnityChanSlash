using UniRx;
using UnityEngine;

public class EnemyStatus : MobStatus
{
    public EnemyType type => (param as EnemyParam).type;
    public Vector3 enemyCorePos => (param as EnemyParam).enemyCore + transform.position;

    protected override void OnActive()
    {
        // Don't reset status on activation
        // Set status by InitParam()

        activeSubject.OnNext(Unit.Default);
    }
}
