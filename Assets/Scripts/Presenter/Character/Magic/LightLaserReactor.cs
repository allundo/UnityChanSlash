using UnityEngine;

[RequireComponent(typeof(LightLaserStatus))]
[RequireComponent(typeof(LightLaserInput))]
[RequireComponent(typeof(LightLaserEffect))]
public class LightLaserReactor : MagicReactor
{
    protected ILauncher subLaserLauncher;

    protected override void Awake()
    {
        base.Awake();
        subLaserLauncher = new Launcher(status, MagicType.SubLaser);
    }

    public void FireSubLaser() => subLaserLauncher.Fire();
}
