public class PlayerFightStyle : MobFightStyle
{
    protected FightStyleHandler handler = null;

    public override IAttack Attack(int index) => handler.Attack(index);

    public FightStyleHandler SetFightStyle(IEquipments equipments)
    {
        if (handler != null) Destroy(handler.gameObject);

        handler = equipments.LoadFightStyle(transform);
        return handler;
    }

    public override void OnDie()
    {
        handler.Attacks.ForEach(atk => atk.OnDie());
    }
}