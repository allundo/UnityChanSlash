public class TrapAttacker : Attacker
{
    public TrapAttacker(float attack, IDirection dir, string name) : base(attack, dir, name) { }
    public override string CauseOfDeath(AttackType type = AttackType.None) => Name + "にハマって絶命した";
}