using UnityEngine;
using UniRx;
using System;

[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(MapUtil))]
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
    [SerializeField] public ItemIconGenerator itemIconGenerator = default;

    /// <summary>
    /// Hide plate handler to update HidePlate positions at players Move and Turn Command execution.
    /// </summary>
    public HidePlateHandler hidePlateHandler { get; protected set; }

    public ISubject<Unit> onClearAll { get; protected set; } = new Subject<Unit>();
    public IObservable<Unit> OnClearAll => onClearAll;

    public ISubject<bool> onUIVisible { get; protected set; } = new Subject<bool>();
    public IObservable<bool> OnUIVisible => onUIVisible;

    protected override void Awake()
    {
        base.Awake();
        hidePlateHandler = GetComponent<HidePlateHandler>();
    }
}
