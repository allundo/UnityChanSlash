using UnityEngine;

[RequireComponent(typeof(MagicStatus))]
[RequireComponent(typeof(MagicInput))]
[RequireComponent(typeof(MagicEffect))]
[RequireComponent(typeof(MapUtil))]
[RequireComponent(typeof(FightStyle))]
public class MagicReactor : MortalReactor
{
    protected IMapUtil map;
    protected IInput input;
    protected IMagicEffect effect;
    protected IMagicAttack attack;

    protected override void Awake()
    {
        base.Awake();
        effect = GetComponent<MagicEffect>();
        input = GetComponent<MagicInput>();
        map = GetComponent<MapUtil>();
        attack = (GetComponent<FightStyle>().Attack(0) as IMagicAttack);
    }

    protected override void OnLifeChange(float life)
    {
        if (life <= 0.0f) input.InterruptDie();
    }

    protected override void OnActive()
    {
        effect.OnActive();
        // Need to set MapUtil.onTilePos before input moving Command
        map.OnActive();
        input.OnActive();
    }

    public override void OnDie()
    {
        attack.SetCollider(false);
        effect.Disappear(OnDead);
    }

    public override void Destroy()
    {
        // Stop all tweens before destroying
        input.ClearAll();
        effect.OnDestroyByReactor();
        attack.SetCollider(false);

        Destroy(gameObject);
    }
}
