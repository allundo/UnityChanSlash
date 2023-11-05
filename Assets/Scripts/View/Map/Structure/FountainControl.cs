using System;
using UnityEngine;
using UniRx;

public class FountainControl : MonoBehaviour
{
    [SerializeField] private ParticleSystem streamVfx = default;
    [SerializeField] private ParticleSystem hotStreamVfx = default;

    protected virtual void Start()
    {
        streamVfx.PlayEx();
    }

    public FountainControl Subscribe(IObservable<bool> eventObservable)
    {
        eventObservable.Subscribe(isEventOn => SwitchStream(isEventOn)).AddTo(this);
        return this;
    }

    protected void SwitchStream(bool isEventOn)
    {
        if (isEventOn)
        {
            streamVfx.StopEmitting();
            hotStreamVfx.PlayEx();
        }
        else
        {
            streamVfx.PlayEx();
            hotStreamVfx.StopEmitting();
        }
    }
}
