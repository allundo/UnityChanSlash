using UniRx;
using System;

public class PitState : IOpenState
{
    private static readonly float[] PIT_DAMAGE = new float[] { 20f, 30f, 40f, 50f, 70f };
    public float damage { get; private set; }
    public ISubject<bool> dropped = new Subject<bool>();
    public IObservable<bool> Dropped => dropped;

    public bool IsOpen => isDropped;
    public bool isDropped { get; protected set; } = false;

    public PitState(int floor)
    {
        this.damage = PIT_DAMAGE[floor - 1];
    }

    public void Open() => Drop(false);
    public void Drop(bool isFXActive = true)
    {
        isDropped = true;
        dropped.OnNext(isFXActive);
        dropped.OnCompleted();
    }
}
