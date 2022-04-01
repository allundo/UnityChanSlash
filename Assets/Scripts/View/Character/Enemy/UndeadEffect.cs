public interface IUndeadEffect : IBodyEffect
{
    void OnResurrection();
}

public class UndeadEffect : MobEffect, IUndeadEffect
{
    public virtual void OnResurrection()
    {
        PlayBodyVFX(VFXType.Resurrection, transform.position);
        PlayBodySnd(SNDType.ResurrectionSkull, transform.position);
    }
}