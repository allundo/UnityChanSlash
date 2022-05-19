using UnityEngine;

public class EventCommandTarget : ICommandTarget
{
    private PlayerCommandTarget target;

    public MobAnimator anim => target.anim;

    public IReactor react => target.react;

    public IInput input => target.input;

    public IMapUtil map => target.map;

    public AttackBehaviour Attack(int index) => target.Attack(index);

    public Magic magic => target.magic;

    public Transform transform => target.transform;

    /// <summary>
    /// Message window controller for EventMessage Command execution.
    /// </summary>
    public MessageController messageController { get; private set; }

    /// <summary>
    /// GamaOverUI to display after PlayerDie Command execution.
    /// </summary>
    public GameOverUI gameOverUI { get; private set; }

    public LightManager lightManager { get; private set; }

    public EventCommandTarget(PlayerCommandTarget target, MessageController messageController, GameOverUI gameOverUI, LightManager lightManager)
    {
        this.target = target;
        this.messageController = messageController;
        this.gameOverUI = gameOverUI;
        this.lightManager = lightManager;
    }

}