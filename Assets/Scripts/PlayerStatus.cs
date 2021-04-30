public class PlayerStatus : MobStatus
{
    protected override float Shield(Direction attackDir) => IsShieldOn(attackDir) ? 1 : 0;
    protected override bool IsShieldOn(Direction attackDir) => commander.anim.GetBool("Guard") && attackDir.IsInverse(dir);

    protected override void OnDamage(float damage, float shield)
    {
        if (shield > 0)
        {
            anim.SetTrigger("Shield");
        }

        base.OnDamage(damage, shield);
    }
}
