using UnityEngine;
using UniRx;

[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(PlayerReactor))]
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

    /// <summary>
    /// Hide plate handler to update HidePlate positions at players Move and Turn Command execution.
    /// </summary>
    public HidePlateHandler hidePlateHandler { get; protected set; }

    public ISubject<bool> inputVisible { get; protected set; } = new Subject<bool>();
    public ISubject<bool> subUIEnable { get; protected set; } = new Subject<bool>();
    public ISubject<Unit> cancel { get; protected set; } = new Subject<Unit>();

    protected override void Awake()
    {
        base.Awake();
        mainCamera.SetLookAt(transform);
        hidePlateHandler = GetComponent<HidePlateHandler>();
    }
}
