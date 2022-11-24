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

    /// <summary>
    /// Message window controller for PlayerMessage Command execution.
    /// </summary>
    [SerializeField] public MessageController messageController = default;

    /// <summary>
    /// GamaOverUI to display after PlayerDie Command execution.
    /// </summary>
    [SerializeField] public GameOverUI gameOverUI = default;

    /// <summary>
    /// Item and item UI icon generator for PlayerHandle Command execution.
    /// </summary>
    [SerializeField] public ItemGenerator itemGenerator = default;

    public ItemInventory itemInventory => (input as PlayerInput).GetItemInventory;

    /// <summary>
    /// Hide plate handler to update HidePlate positions at players Move and Turn Command execution.
    /// </summary>
    public HidePlateHandler hidePlateHandler { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        mainCamera.SetLookAt(transform);
        hidePlateHandler = GetComponent<HidePlateHandler>();
    }
}
