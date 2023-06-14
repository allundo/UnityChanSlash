using UnityEngine;

[RequireComponent(typeof(LightLaserStatus))]
[RequireComponent(typeof(LightLaserInput))]
[RequireComponent(typeof(LightLaserEffect))]
public class LightLaserReactor : MagicReactor
{
    protected ILauncher subLaserLauncher;
    protected LightLaserEffect laserEffect;

    protected override void Awake()
    {
        base.Awake();
        subLaserLauncher = new Launcher(status, MagicType.SubLaser);
        laserEffect = effect as LightLaserEffect;
    }

    protected override void OnActive()
    {
        laserEffect.SetLength((status as LightLaserStatus).length);
        base.OnActive();
    }

    public void FireSubLaser() => subLaserLauncher.Fire();
}
