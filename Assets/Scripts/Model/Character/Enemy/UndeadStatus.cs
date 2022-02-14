using UnityEngine;
using System.Collections.Generic;

public class UndeadStatus : EnemyStatus
{
    public float curse { get; protected set; }
    protected float curseMax;

    protected static Dictionary<AttackAttr, float> curseMultiplier
        = new Dictionary<AttackAttr, float>()
        {
            { AttackAttr.None,       -1f },
            { AttackAttr.Fire,      0.5f },
            { AttackAttr.Ice,         0f },
            { AttackAttr.Thunder,     0f },
            { AttackAttr.Light,       2f },
            { AttackAttr.Dark,       -2f },
        };

    protected override void Awake()
    {
        base.Awake();
        curse = curseMax = 0f;
    }

    public override void ResetStatus()
    {
        curse = curseMax = DefaultLifeMax;
        base.ResetStatus();
    }

    public override void Damage(float damage, AttackAttr attr = AttackAttr.None)
    {
        curse = Mathf.Clamp(curse - damage * curseMultiplier[attr], 0f, curseMax);
        life.Value -= damage;
    }
}
