public class DarkSpiritReactor : HealSpiritReactor
{
    protected override void AffectTarget(IMobReactor target)
    {
        target.Damage(status.attack, Direction.Convert(transform.forward), AttackType.Dark, AttackAttr.Dark);
    }
}