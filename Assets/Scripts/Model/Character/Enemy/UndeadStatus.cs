using UnityEngine;
using System.Collections.Generic;

public interface IUndeadStatus : IEnemyStatus
{
    float curse { get; }
}

public class UndeadStatus : EnemyStatus, IUndeadStatus
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
        base.ResetStatus();
        curse = curseMax = lifeMax.Value;
    }

    public override void LifeChange(float diff, AttackAttr attr = AttackAttr.None)
    {
        curse = Mathf.Clamp(curse + diff * curseMultiplier[attr], 0f, curseMax);
        life.Value = Mathf.Clamp(life.Value + diff, 0f, lifeMax.Value);
    }
}
