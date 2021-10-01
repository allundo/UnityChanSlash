using UnityEngine;
using UniRx;
using System;

[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(MapUtil))]
[RequireComponent(typeof(HidePlateHandler))]
public class PlayerCommander : ShieldCommander
{
    [SerializeField] public ThirdPersonCamera mainCamera = default;
    [SerializeField] public MobAttack jab = default;
    [SerializeField] public MobAttack straight = default;
    [SerializeField] public MobAttack kick = default;
    [SerializeField] public MessageController messageController = default;
    [SerializeField] public GameOverUI gameOverUI = default;
    [SerializeField] public ItemGenerator itemGenerator = default;
    [SerializeField] public ItemIconGenerator itemIconGenerator = default;

    public HidePlateHandler hidePlateHandler { get; protected set; }

    public bool IsAttack => currentCommand is PlayerAttack;

    public ISubject<Unit> onValidateTrigger { get; protected set; } = new Subject<Unit>();
    protected IObservable<Unit> OnValidateTrigger => onValidateTrigger;

    public ISubject<Unit> onClearAll { get; protected set; } = new Subject<Unit>();
    protected IObservable<Unit> OnClearAll => onClearAll;

    public ISubject<bool> onUIVisible { get; protected set; } = new Subject<bool>();
    public IObservable<bool> OnUIVisible => onUIVisible;

    protected override void Awake()
    {
        base.Awake();
        hidePlateHandler = GetComponent<HidePlateHandler>();
    }

    protected override void Start()
    {
        OnCompleted.Subscribe(_ => DispatchCommand()).AddTo(this);
        OnClearAll.Subscribe(_ => ClearAll()).AddTo(this);
    }
}
