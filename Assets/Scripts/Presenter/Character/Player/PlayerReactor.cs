using UnityEngine;

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
        playerInput = GetComponent<PlayerInput>();
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

    protected override void OnDie()
    {
        base.OnDie();
        playerInput.SetInputVisible(false);
    }

    protected override void Dead() => base.Inactivate();
}
