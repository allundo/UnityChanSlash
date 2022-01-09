public class EnemyStatus : MobStatus
{
    public EnemyType type => (param as EnemyParam).type;

    public override void Activate()
    {
        if (isActive) return;

        isActive = true;
        gameObject.SetActive(true);

        // Don't reset status on activation
        // Set status by InitParam()
    }
}
