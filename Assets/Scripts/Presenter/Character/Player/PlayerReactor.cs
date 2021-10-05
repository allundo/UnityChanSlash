using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerStatus))]
public class PlayerReactor : MobReactor
{
    protected GuardStateTemp guardState => playerInput.guardState;
    protected PlayerInput playerInput;

    protected override void Awake()
    {
        base.Awake();
        playerInput = GetComponent<PlayerInput>();
    }

    protected override float CalcDamage(float attack, IDirection dir)
    {
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
