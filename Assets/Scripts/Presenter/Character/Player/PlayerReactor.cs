using UnityEngine;
using UniRx;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerStatus))]
public class PlayerReactor : MobReactor
{
    [SerializeField] protected RestUI restUI = default;

    protected GuardState guardState => playerInput.guardState;
    protected PlayerInput playerInput;

    protected override void Awake()
    {
        base.Awake();
        playerInput = input as PlayerInput;
    }

    protected override void Start()
    {
        base.Start();
        restUI.Heal.Subscribe(point => OnHealRatio(point, false)).AddTo(this);
    }

    public override void OnHealRatio(float healRatio = 0f, bool isEffectOn = true)
    {
        if (healRatio == 0f)
        {
            // Update RestUI life gauge
            restUI.OnLifeChange(status.Life.Value, status.LifeMax.Value);
            return;
        }

        base.OnHealRatio(healRatio, isEffectOn);
    }
    protected override void OnLifeChange(float life)
    {
        base.OnLifeChange(life);
        restUI.OnLifeChange(life, status.LifeMax.Value);
    }

    public override void OnDamage(float attack, IDirection dir)
    {
        base.OnDamage(attack, dir);
        if (restUI.isActive) restUI.OnDamage();
    }

    protected override float CalcDamage(float attack, IDirection dir)
    {
        if (restUI.isActive) return Mathf.Max(status.CalcAttack(attack, null), 0.0f);

        float shield = 0.0f;

        if (guardState.IsShieldOn(dir))
        {
            shield = status.Shield;
            guardState.SetShield();
        }

        return Mathf.Max(status.CalcAttack(attack, dir) - shield, 0.0f);
    }
}
