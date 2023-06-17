using UnityEngine;
using System.Collections.Generic;

public interface IUndeadStatus : IEnemyStatus
{
    float curse { get; }
}

public class UndeadStatus : EnemyStatus, IUndeadStatus
{
    public float curse { get; protected set; } = 0f;

    protected static Dictionary<AttackAttr, float> curseMultiplier
        = new Dictionary<AttackAttr, float>()
        {
            { AttackAttr.None,       -1f },
            { AttackAttr.Fire,      0.5f },
            { AttackAttr.Ice,         0f },
            { AttackAttr.Thunder,     0f },
            { AttackAttr.Light,       2f },
            { AttackAttr.Dark,       -2f },
            { AttackAttr.Coin,        0f },
        };

    public override void LifeChange(float diff, AttackAttr attr = AttackAttr.None)
    {
        curse = Mathf.Clamp(curse + diff * curseMultiplier[attr], 0f, lifeMax.Value);
        life.Value = Mathf.Clamp(life.Value + diff, 0f, lifeMax.Value);
    }

    protected override IEnemyStatus InitParam(EnemyParam param, EnemyStoreData data)
    {
        base.InitParam(param, data);

        curse = data == null || data.curse == 0 ? lifeMax.Value : data.curse;

        return this;
    }
}
