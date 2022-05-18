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

    void Awake()
    {
        col = GetComponent<Collider>();
        SetEnabled(false);
    }

    public void SetEnabled(bool isEnabled = true)
    {
        col.enabled = isEnabled;
    }

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
        SetEnabled(false);
        gameObject.SetActive(true);
    }

    public override void Inactivate()
    {
        detectPlayer.OnCompleted();
        gameObject.SetActive(false);
    }

    public EventInvoker Init(Func<PlayerCommandTarget, bool> IsEventValid, bool isOneShot = true)
    {
        this.IsEventValid = IsEventValid;
        this.isOneShot = isOneShot;
        return this;
    }
}