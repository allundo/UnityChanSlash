using UnityEngine;

/// <summary>
/// Keeps attached component used by Command execution.
/// </summary>
[RequireComponent(typeof(Reactor))]
[RequireComponent(typeof(InputHandler))]
[RequireComponent(typeof(MapUtil))]
[RequireComponent(typeof(FightStyle))]
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
    /// Attack collider and type handlers invoked on attack commands.
    /// </summary>
    public IAttack Attack(int index) => fightStyle.Attack(index);

    /// <summary>
    /// Bullet attack source. Not imperative.
    /// </summary>
    public Magic magic { get; protected set; }

    protected FightStyle fightStyle;

    protected virtual void Awake()
    {
        anim = GetComponent<MobAnimator>();
        react = GetComponent<Reactor>();
        input = GetComponent<InputHandler>();
        map = GetComponent<MapUtil>();
        magic = GetComponent<Magic>();
        fightStyle = GetComponent<FightStyle>();
    }
}

