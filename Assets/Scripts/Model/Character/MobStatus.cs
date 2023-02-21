using UnityEngine;
using System.Collections.Generic;

public interface IMobStatus : IStatus
{
    float Shield { get; }
    int level { get; }
    bool isOnGround { get; }
    bool isHidden { get; }
    void SetHidden(bool isHidden = true);
    float icingFrames { get; }
    float SetIcingFrames(float framesToMelt);
    float UpdateIcingFrames();

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

    public int level { get; protected set; }
    protected LevelGain levelGain;

    protected float GainedLifeMax => param.defaultLifeMax * (1f + 0.25f * level) * Mathf.Pow(levelGain.lifeMaxGainRatio, level);
    public virtual float Shield => mobParam.shield + levelGain.shieldGain * level;

    public bool isHidden { get; protected set; }

    public virtual void SetHidden(bool isHidden = true)
    {
        this.isHidden = isHidden;
    }

    public float icingFrames { get; protected set; }

    protected virtual float ArmorMultiplier => mobParam.armorMultiplier - levelGain.armorReduction * level;
    public override float MagicMultiplier => mobParam.magic + levelGain.magicGain * level;

    public virtual Vector3 corePos => transform.position;

    public virtual float CalcAttack(float attack, IDirection attackDir, AttackAttr attr = AttackAttr.None)
    {
        var attrDM = CalcAttrDM(attr);

        // Direction doesn't affect attack power when absorbing attribute of the attack.
        var dirDM = attrDM > 0f ? dirDamageMultiplier[GetDamageType(attackDir)] : 1f;

        return attack * ArmorMultiplier * dirDM * attrDM;
    }
    /// <summary>
    /// Reduce damage by default attribute damage multiplier and magic power.
    /// </summary>
    /// <param name="attr">damage attribute</param>
    /// <returns>damage multiplier based on the attribute</returns>
    protected virtual float CalcAttrDM(AttackAttr attr)
    {
        return attrDamageMultiplier[attr] / (0.5f * (1f + MagicMultiplier));

    }

    protected DamageType GetDamageType(IDirection attackerDir)
    {
        if (attackerDir == null || icingFrames > 0f) return DamageType.Rest;

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

    public override void ResetStatus(float life = 0f)
    {
        lifeMax.Value = GainedLifeMax;
        this.life.Value = life == 0f ? lifeMax.Value : life;

        isOnGround = mobParam.isOnGround;
        icingFrames = 0f;
        isHidden = false;
    }

    public override IStatus InitParam(Param param, StatusStoreData data = null)
        => InitParam(param as MobParam, data as MobStatusStoreData);

    private IMobStatus InitParam(MobParam mobParam, MobStatusStoreData data)
    {
        this.param = this.mobParam = mobParam;
        data = data ?? new MobStatusStoreData();

        this.level = data.level;

        dirDamageMultiplier = new Dictionary<DamageType, float>()
        {
            { DamageType.Face,        mobParam.faceDamageMultiplier },
            { DamageType.Side,        mobParam.sideDamageMultiplier },
            { DamageType.Back,        mobParam.backDamageMultiplier },
            { DamageType.Rest,        mobParam.restDamageMultiplier },
        };

        attrDamageMultiplier = new Dictionary<AttackAttr, float>()
        {
            { AttackAttr.None,        1f                                                                        },
            { AttackAttr.Fire,        mobParam.fireDamageMultiplier     - (levelGain.fireReduction * level)     },
            { AttackAttr.Ice,         mobParam.iceDamageMultiplier      - (levelGain.iceReduction * level)      },
            { AttackAttr.Thunder,     mobParam.thunderDamageMultiplier  - (levelGain.thunderReduction * level)  },
            { AttackAttr.Light,       mobParam.lightDamageMultiplier    - (levelGain.lightReduction * level)    },
            { AttackAttr.Dark,        mobParam.darkDamageMultiplier     - (levelGain.darkReduction * level)     },
            { AttackAttr.Coin,        1f                                                                        },
        };

        ResetStatus(data.life);
        attack = mobParam.attack + levelGain.attackGain * level;

        return this;
    }

    public float SetIcingFrames(float icingFrames) => this.icingFrames = icingFrames;

    public float UpdateIcingFrames()
    {
        icingFrames = GetComponent<MobInput>().GetIcingFrames();
        return icingFrames;
    }
}
