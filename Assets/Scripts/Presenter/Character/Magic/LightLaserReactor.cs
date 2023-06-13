using UnityEngine;

[RequireComponent(typeof(LightLaserStatus))]
[RequireComponent(typeof(LightLaserInput))]
[RequireComponent(typeof(LightLaserEffect))]
public class LightLaserReactor : BulletReactor
{
    protected ILauncher subLaserLauncher;

    protected override void Awake()
    {
        base.Awake();
        subLaserLauncher = new Launcher(status, BulletType.SubLaser);
    }

    public void FireSubLaser() => subLaserLauncher.Fire();
}
