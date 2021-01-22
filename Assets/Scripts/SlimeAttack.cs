using UnityEngine;

public class SlimeAttack : MobAttack {

    [SerializeField ]protected ParticleSystem vfx = default;

    protected override float Pitch => 1.2f;

    public override void OnAttackStart()
    {
        base.OnAttackStart();

        if (vfx != null)
        {
            vfx.Play();
        }
    }
}
