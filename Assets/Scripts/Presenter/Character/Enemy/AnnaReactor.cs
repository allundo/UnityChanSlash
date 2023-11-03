public class AnnaReactor : ShieldEnemyReactor
{
    public override void OnOutOfView()
    {
        // Don't disappear.
    }

    protected override float CalcDamage(float attack, IDirection dir, AttackAttr attr)
    {
        var damage = mobStatus.CalcAttack(attack, dir, attr);
        // Apply armor to Slash skill.
        if (input.currentCommand is AnnaSlash) damage *= 0.25f;
        return damage;
    }
}
