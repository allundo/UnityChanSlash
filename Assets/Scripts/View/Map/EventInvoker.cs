using UnityEngine;
using UniRx;
using System;

[RequireComponent(typeof(Collider))]
public class EventInvoker : SpawnObject<EventInvoker>
{
    public IObservable<Unit> DetectPlayer => detectPlayer;
    private ISubject<Unit> detectPlayer = new Subject<Unit>();

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerCommandTarget>() == null) return;

        detectPlayer.OnNext(Unit.Default);
    }

    public override EventInvoker OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0f)
    {
        transform.position = pos;
        Activate();
        return this;
    }
}