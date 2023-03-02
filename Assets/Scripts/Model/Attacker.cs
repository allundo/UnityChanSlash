public interface IAttacker
{
    float attack { get; }
    IDirection dir { get; }
    string Name { get; }
    string CauseOfDeath(AttackType type = AttackType.None);
}

public class Attacker : IAttacker
{
    public float attack { get; protected set; }
    public IDirection dir { get; protected set; }
    public string Name => name;
    protected string name;
    public virtual string CauseOfDeath(AttackType type = AttackType.None) => Name + "にやられた";

    public Attacker(float attack, IDirection dir, string name)
    {
        this.attack = attack;
        this.dir = dir;
        this.name = name;
    }
}
