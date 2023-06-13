using System;
using UniRx;

public class SubLaserStatus : MagicStatus, ILaserStatus
{
    public int length { get; private set; } = 0;

    public override MagicStatus SetShooter(IStatus status)
    {
        base.SetShooter(status);
        length = (status as ILaserStatus).length;

        return this;
    }
}
