using UnityEngine;
using System.Collections.Generic;

public interface IMobStatus : IStatus
{
    float Shield { get; }
    bool isOnGround { get; }
    bool isHidden { get; }
    void SetHidden(bool isHidden = true);
    bool isIced { get; }
    void SetIsIced(bool isIced);

    Vector3 corePos { get; }

    float CalcAttack(float attack, IDirection attackDir, AttackAttr attr = AttackAttr.None);
}

public class MobStatus : Status, IMobStatus
{
    protected MobParam mobParam;

    protected enum DamageType
    {
        Face,
        Side,
        Back,
        Rest,
    }

    protected Dictionary<DamageType, float> dirDamageMultiplier;
    protected Dictionary<AttackAttr, float> attrDamageMultiplier;

    public virtual float Shield => mobParam.shield;

    public bool isHidden { get; protected set; }

    public virtual void SetHidden(bool isHidden = true)
    {
        this.isHidden = isHidden;
    }

    public bool isIced { get; protected set; }

    public void SetIsIced(bool isIced)
    {
        this.isIced = isIced;
    }

    protected virtual float ArmorMultiplier => mobParam.armorMultiplier;

    public virtual Vector3 corePos => transform.position;

    public virtual float CalcAttack(float attack, IDirection attackDir, AttackAttr attr = AttackAttr.None)
    {
        return attack * ArmorMultiplier * dirDamageMultiplier[GetDamageType(attackDir)] * attrDamageMultiplier[attr];
    }

    protected DamageType GetDamageType(IDirection attackerDir)
    {
        if (attackerDir == null || isIced) return DamageType.Rest;

        if (attackerDir.IsInverse(dir))
        {
            return DamageType.Face;
        }

        if (attackerDir.IsSame(dir))
        {
            return DamageType.Back;
        }

        return DamageType.Side;
    }

    public override void ResetStatus()
    {
        life.Value = lifeMax.Value = DefaultLifeMax;
        isOnGround = mobParam.isOnGround;
        isIced = isHidden = false;
    }

    public override IStatus InitParam(Param param, float life = 0f)
    {
        mobParam = param as MobParam;

        dirDamageMultiplier = new Dictionary<DamageType, float>()
        {
            { DamageType.Face,        mobParam.faceDamageMultiplier },
            { DamageType.Side,        mobParam.sideDamageMultiplier },
            { DamageType.Back,        mobParam.backDamageMultiplier },
            { DamageType.Rest,        mobParam.restDamageMultiplier },
        };

        attrDamageMultiplier = new Dictionary<AttackAttr, float>()
        {
            { AttackAttr.None,        1f                                },
            { AttackAttr.Fire,        mobParam.fireDamageMultiplier     },
            { AttackAttr.Ice,         mobParam.iceDamageMultiplier      },
            { AttackAttr.Thunder,     mobParam.thunderDamageMultiplier  },
            { AttackAttr.Light,       mobParam.lightDamageMultiplier    },
            { AttackAttr.Dark,        mobParam.darkDamageMultiplier     },
        };

        return base.InitParam(param, life);
    }
}
