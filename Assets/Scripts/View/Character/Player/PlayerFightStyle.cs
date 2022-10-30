public class PlayerFightStyle : MobFightStyle
{
    protected PlayerAnimator anim;
    protected FightStyleHandler handler = null;

    public float ShieldRatioR => handler.ShieldRatioR;
    public float ShieldRatioL => handler.ShieldRatioL;

    protected virtual void Awake()
    {
        anim = GetComponent<PlayerAnimator>();
    }

    public override IAttack Attack(int index) => handler.Attack(index);

    public FightStyleHandler SetFightStyle(IEquipmentStyle equipments)
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