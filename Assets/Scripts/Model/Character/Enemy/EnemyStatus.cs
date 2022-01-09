using UniRx;

public class EnemyStatus : MobStatus
{
    public EnemyType type => (param as EnemyParam).type;

    protected override void OnActive()
    {
        // Don't reset status on activation
        // Set status by InitParam()

        activeSubject.OnNext(Unit.Default);
    }
}
