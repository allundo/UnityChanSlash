public class MobAttackTweenParticle : MobAttackFX
{
    protected TweenParticle tpAttackVFX;
    protected override void Start()
    {
        base.Start();
        tpAttackVFX = attackFX.GetComponent<TweenParticle>();
    }

    public override void OnDie()
    {
        tpAttackVFX.StopAndClear();
    }

    protected override void OnFXStart()
    {
        tpAttackVFX.Play();
        attackSnd.PlayEx();
    }
}