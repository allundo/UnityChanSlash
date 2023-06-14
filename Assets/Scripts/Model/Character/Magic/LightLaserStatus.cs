using System;
using UniRx;

public interface ILaserStatus : IMagicStatus
{
    int length { get; }
}

public class LightLaserStatus : MagicStatus, ILaserStatus
{
    public static readonly int MAX_LENGTH = 9;
    private readonly float CANCEL_TIMER_SEC = 1.2f;
    private WorldMap map;

    public int length { get; private set; } = 0;

    public IObservable<Unit> ShooterDeath => shotBy.Life.Where(life => life <= 0).Select(_ => Unit.Default);

    private IDisposable shooterDeath;
    private IDisposable cancelTimer;

    protected override void Awake()
    {
        base.Awake();
        map = GameManager.Instance.worldMap;
    }

    public override MagicStatus SetShooter(IStatus status)
    {
        base.SetShooter(status);
        length = CalcLength();

        shooterDeath?.Dispose();
        shooterDeath = shotBy.Life.Where(life => life <= 0.0f)
            .Subscribe(_ => life.Value = 0f)
            .AddTo(this);

        cancelTimer?.Dispose();
        cancelTimer = Observable.Timer(TimeSpan.FromSeconds(CANCEL_TIMER_SEC))
            .Subscribe(_ => shooterDeath?.Dispose())
            .AddTo(this);

        return this;
    }

    public override void Inactivate()
    {
        shooterDeath?.Dispose();
        cancelTimer?.Dispose();

        base.Inactivate();
    }

    private int CalcLength() => CalcLength(map.MapPos(shotBy.Position), 0);

    private int CalcLength(Pos pos, int length)
    {
        if (length == MAX_LENGTH) return MAX_LENGTH;

        Pos nextPos = shotBy.dir.GetForward(pos);
        ITile nextTile = map.GetTile(nextPos);

        if (nextTile.IsViewOpen || nextTile is Door)
        {
            return CalcLength(nextPos, length + 1);
        }

        return length;
    }
}
