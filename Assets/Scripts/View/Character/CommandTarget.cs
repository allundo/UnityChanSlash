using UnityEngine;
using System;
using UniRx;

/// <summary>
/// Keeps attached component used by Command execution.
/// </summary>
[RequireComponent(typeof(MobAnimator))]
[RequireComponent(typeof(MobReactor))]
[RequireComponent(typeof(MapUtil))]
public class CommandTarget : MonoBehaviour
{
    /// <summary>
    /// Animation handler for Command execution.
    /// </summary>
    public MobAnimator anim { get; protected set; }

    /// <summary>
    /// Reaction handler for Command execution.
    /// </summary>
    public MobReactor react { get; protected set; }

    /// <summary>
    /// Direction related data for Command execution.
    /// </summary>
    public MapUtil map { get; protected set; } = default;

    protected virtual void Awake()
    {
        anim = GetComponent<MobAnimator>();
        react = GetComponent<MobReactor>();
        map = GetComponent<MapUtil>();
    }
}
