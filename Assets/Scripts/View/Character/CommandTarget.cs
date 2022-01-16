using UnityEngine;

/// <summary>
/// Keeps attached component used by Command execution.
/// </summary>
[RequireComponent(typeof(MobReactor))]
[RequireComponent(typeof(MobInput))]
[RequireComponent(typeof(MapUtil))]
[RequireComponent(typeof(MobAnimator))]
public class CommandTarget : MonoBehaviour
{
    /// <summary>
    /// Animation handler for Command execution.
    /// </summary>
    public MobAnimator anim { get; protected set; }

    /// <summary>
    /// Reaction handler for Command execution.
    /// </summary>
    public IReactor react { get; protected set; }

    /// <summary>
    /// Input handler for Command execution.
    /// </summary>
    public IInput input { get; protected set; }

    /// <summary>
    /// Direction related data for Command execution.
    /// </summary>
    public IMapUtil map { get; protected set; }

    /// <summary>
    /// Bullet attack source. Not imperative.
    /// </summary>
    public Fire fire { get; protected set; }

    protected virtual void Awake()
    {
        anim = GetComponent<MobAnimator>();
        react = GetComponent<MobReactor>();
        input = GetComponent<MobInput>();
        map = GetComponent<MapUtil>();
        fire = GetComponent<Fire>();
    }
}
