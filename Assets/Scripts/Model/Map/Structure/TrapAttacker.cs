public class TrapAttacker : Attacker
{
    public TrapAttacker(float attack, IDirection dir, string name) : base(attack, dir, name, "にハマって絶命した") { }
}
