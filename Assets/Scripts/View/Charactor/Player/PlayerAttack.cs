using UnityEngine;

public class PlayerAttack : MobAttack
{

    [SerializeField] protected ParticleSystem vfx = default;

    protected override float Pitch => 1.2f;

    public override void OnAttackStart()
    {
        base.OnAttackStart();

        vfx?.Play();
    }
}
