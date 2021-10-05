using UnityEngine;
using System;
using UniRx;
using System.Collections.Generic;

/// <summary>
/// Handles Command queuing and dispatching. <br />
/// Keeps attached component data used by Command execution.
/// </summary>
[RequireComponent(typeof(MobAnimator))]
[RequireComponent(typeof(MapUtil))]
public class CommandTarget : MonoBehaviour
{
    /// <summary>
    /// Animation handler for Command execution.
    /// </summary>
    public MobAnimator anim { get; protected set; }

    /// <summary>
    /// Direction related data for Command execution.
    /// </summary>
    public MapUtil map { get; protected set; } = default;

    /// <summary>
    /// Notify the end of DieCommand execution.
    /// </summary>
    public ISubject<Unit> onDead { get; protected set; } = new Subject<Unit>();
    public IObservable<Unit> OnDead => onDead;

    protected virtual void Awake()
    {
        anim = GetComponent<MobAnimator>();
        map = GetComponent<MapUtil>();
    }
}
