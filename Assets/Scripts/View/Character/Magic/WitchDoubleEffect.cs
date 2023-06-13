public class WitchDoubleEffect : BulletEffect
{
    protected override void Awake()
    {
        bulletMatEffect = new MatColorEffect(meshTf);
    }

    public void OnAttackStart()
    {
        emitVfx.PlayEx();
    }

    public void OnAttackEnd()
    {
        emitVfx.StopEmitting();
    }

    public override void OnActive()
    {
        bulletMatEffect.Activate(0.01f);
    }

    protected override void OnDisappear() { }

    public override void OnHit() { }
}
