using UnityEngine;

public interface IEnemyEffect : IMobEffect
{
    void OnActive(float duration);

    // For summon monster
    void SummonFX();
    void OnTeleportEnd();
    void ShowGauge(float valueRatio);
    void HideGauge();
}

public class EnemyEffect : MobEffect, IEnemyEffect
{

    [SerializeField] private PanelLifeGauge gauge = default;

    public virtual void OnActive(float duration)
    {
        matColEffect.Activate(duration);
        gauge.Disable();
    }

    /// <summary>
    /// Plays effect for summoned enemy. Use TeleportDest VFX commonly. <br />
    /// Needs to call OnTeleportEnd() to stop this effect.
    /// </summary>
    public void SummonFX()
    {
        resourceFX.PlayVFX(VFXType.TeleportDest, transform.position);
        resourceFX.PlaySnd(SNDType.TeleportDest, transform.position);
    }

    public void OnTeleportEnd()
    {
        resourceFX.StopVFX(VFXType.TeleportDest);
    }

    public void ShowGauge(float valueRatio)
    {
        gauge.UpdateGauge(valueRatio);
        gauge.FadeActivate();
    }

    public void HideGauge() => gauge.FadeInactivate();
}
