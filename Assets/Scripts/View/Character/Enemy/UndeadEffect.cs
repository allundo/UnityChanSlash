public interface IUndeadEffect : IBodyEffect
{
    void OnResurrection();
}

public class UndeadEffect : EnemyEffect, IUndeadEffect
{
    public virtual void OnResurrection()
    {
        resourceFX.PlayVFX(VFXType.Resurrection, transform.position);
        resourceFX.PlaySnd(SNDType.ResurrectionSkull, transform.position);
    }
}