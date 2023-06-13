using System;
using UniRx;

public class SubLaserStatus : BulletStatus, ILaserStatus
{
    public int length { get; private set; } = 0;

    public override BulletStatus SetShooter(IStatus status)
    {
        base.SetShooter(status);
        length = (status as ILaserStatus).length;

        return this;
    }
}
