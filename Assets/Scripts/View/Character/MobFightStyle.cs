public class MobFightStyle : FightStyle
{
    public virtual void OnDie()
    {
        attacks.ForEach(atk => (atk as IMobAttack).OnDie());
    }
}