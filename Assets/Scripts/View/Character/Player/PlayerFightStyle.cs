public class PlayerFightStyle : MobFightStyle
{
    protected PlayerAnimator anim;
    protected FightStyleHandler handler = null;

    protected virtual void Awake()
    {
        anim = GetComponent<PlayerAnimator>();
    }

    public override IAttack Attack(int index) => handler.Attack(index);

    public FightStyleHandler SetFightStyle(IEquipments equipments)
    {
        if (handler != null) Destroy(handler.gameObject);

        handler = equipments.LoadFightStyle(transform);
        anim.SetController(equipments.animatorController);
        return handler;
    }

    public override void OnDie()
    {
        handler.Attacks.ForEach(atk => atk.OnDie());
    }
}