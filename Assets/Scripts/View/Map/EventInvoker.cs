using UnityEngine;
using UniRx;
using System;

[RequireComponent(typeof(Collider))]
public class EventInvoker : SpawnObject<EventInvoker>
{
    public IObservable<Unit> DetectPlayer => detectPlayer;
    private ISubject<Unit> detectPlayer = new Subject<Unit>();

    private Collider col;
    private bool isOneShot = true;
    private Func<PlayerCommandTarget, bool> IsEventValid;
    private Pos pos;

    void Awake()
    {
        col = GetComponent<Collider>();
        col.enabled = false;
    }

    public void Enable(WorldMap map)
    {
        transform.position = map.WorldPos(pos);
        col.enabled = true;
    }

    public void Disable() => col.enabled = false;

    public void OnTriggerEnter(Collider other)
    {
        var target = other.GetComponent<PlayerCommandTarget>();
        if (target == null || !IsEventValid(target)) return;

        detectPlayer.OnNext(Unit.Default);

        if (isOneShot) Inactivate();
    }

    public override EventInvoker OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0f)
    {
        transform.position = pos;
        Activate();
        return this;
    }

    public override void Activate()
    {
        col.enabled = false;
        gameObject.SetActive(true);
    }

    public override void Inactivate()
    {
        detectPlayer.OnCompleted();
        gameObject.SetActive(false);
    }

    public EventInvoker Init(Pos pos, Func<PlayerCommandTarget, bool> IsEventValid, bool isOneShot = true)
    {
        this.pos = pos;
        this.IsEventValid = IsEventValid;
        this.isOneShot = isOneShot;
        return this;
    }
}