using UnityEngine;

[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(PlayerReactor))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerMapUtil))]
[RequireComponent(typeof(HidePlateHandler))]
public class PlayerCommandTarget : CommandTarget
{
    /// <summary>
    /// Player trailing Camera for PlayerTurn Command execution.
    /// </summary>
    [SerializeField] public ThirdPersonCamera mainCamera = default;

    // Player attack handler for PlayerAttack Command execution.
    [SerializeField] public MobAttack jab = default;
    [SerializeField] public MobAttack straight = default;
    [SerializeField] public MobAttack kick = default;

    /// <summary>
    /// Message window controller for PlayerMessage Command execution.
    /// </summary>
    [SerializeField] public MessageController messageController = default;

    /// <summary>
    /// GamaOverUI to display after PlayerDie Command execution.
    /// </summary>
    [SerializeField] public GameOverUI gameOverUI = default;

    // Item and item UI icon generator for PlayerHandle Command execution.
    [SerializeField] public ItemGenerator itemGenerator = default;
    [SerializeField] public ItemInventory itemInventory = default;

    /// <summary>
    /// Hide plate handler to update HidePlate positions at players Move and Turn Command execution.
    /// </summary>
    public HidePlateHandler hidePlateHandler { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        hidePlateHandler = GetComponent<HidePlateHandler>();
    }
}
