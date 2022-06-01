using UniRx;
using System;

public class PitState
{
    public float damage { get; private set; }
    public ISubject<bool> dropped = new Subject<bool>();
    public IObservable<bool> Dropped => dropped;

    public bool isDropped { get; protected set; } = false;

    public PitState(float damage = 1f)
    {
        this.damage = damage;
    }

    public void Drop(bool isFXActive = true)
    {
        isDropped = true;
        dropped.OnNext(isFXActive);
        dropped.OnCompleted();
    }
}
