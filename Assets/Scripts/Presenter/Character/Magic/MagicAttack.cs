public interface IMagicAttack : IAttack
{
    void SetCollider(bool isEnabled);
}

public class MagicAttack : Attack, IMagicAttack
{
    protected AttackData data;

    protected override void Awake()
    {
        base.Awake();
        data = new AttackData(attackMultiplier, attackType, attackAttr);
    }

    protected override void AffectTarget(IMobReactor target)
    {
        var magicStatus = status as MagicStatus;
        target.Damage(Shooter.New(magicStatus.Attack, magicStatus.shotBy), data);
    }

    public void SetCollider(bool isEnabled)
    {
        attackCollider.enabled = isEnabled;
    }
}
