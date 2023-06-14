using UnityEngine;

public class DarkSpiritReactor : HealSpiritReactor
{
    protected Attack.AttackData data;
    protected Vector3 prevPos;

    protected override void Awake()
    {
        base.Awake();
        data = new Attack.AttackData(1, AttackType.Dark, AttackAttr.Dark);
    }

    protected override void Update()
    {
        prevPos = transform.position;
        base.Update();
    }

    protected override void AffectTarget(IMobReactor target)
    {
        target.Damage(new Attacker(status.attack, Direction.Convert(transform.position - prevPos), "呪いの魂", "の怨念に倒れた"), data);
    }
}
