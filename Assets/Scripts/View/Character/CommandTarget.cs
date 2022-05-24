using UnityEngine;

public interface ICommandTarget
{
    /// <summary>
    /// Animation handler for Command execution.
    /// </summary>
    MobAnimator anim { get; }

    /// <summary>
    /// Reaction handler for Command execution.
    /// </summary>
    IReactor react { get; }

    /// <summary>
    /// Input handler for Command execution.
    /// </summary>
    IInput input { get; }

    /// <summary>
    /// Direction related data for Command execution.
    /// </summary>
    IMapUtil map { get; }

    /// <summary>
    /// Attack collider and type handlers invoked on attack commands.
    /// </summary>
    AttackBehaviour Attack(int index);

    /// <summary>
    /// Bullet attack source. Not imperative.
    /// </summary>
    Magic magic { get; }

    Transform transform { get; }
}

/// <summary>
/// Keeps attached component used by Command execution.
/// </summary>
[RequireComponent(typeof(Reactor))]
[RequireComponent(typeof(InputHandler))]
[RequireComponent(typeof(MapUtil))]
public class CommandTarget : MonoBehaviour, ICommandTarget
{
    public MobAnimator anim { get; protected set; }

    public IReactor react { get; protected set; }

    public IInput input { get; protected set; }

    public IMapUtil map { get; protected set; }

    [SerializeField] private AttackBehaviour[] attack = default;
    public AttackBehaviour Attack(int index) => attack[index];

    public Magic magic { get; protected set; }

    protected virtual void Awake()
    {
        anim = GetComponent<MobAnimator>();
        react = GetComponent<Reactor>();
        input = GetComponent<InputHandler>();
        map = GetComponent<MapUtil>();
        magic = GetComponent<Magic>();
    }
}

