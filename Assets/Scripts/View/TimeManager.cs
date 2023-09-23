using UnityEngine;
using UniRx;

public class TimeManager : SingletonMonoBehaviour<TimeManager>
{
    [SerializeField] private PlayerInput playerInput = default;
    [SerializeField] private MessageController messageController = default;

    public IPlayerInput input;

    public bool isPaused { get; private set; } = false;
    public bool isScaled { get; private set; } = false;

    public float elapsedTimeSecBuffer { get; private set; } = 0f;
    public int elapsedTimeSec { get; private set; } = 0;

    public void AddTimeSec(int timeSec)
    {
        elapsedTimeSec += timeSec;
    }

    protected override void Awake()
    {
        base.Awake();
        input = playerInput;
    }

    void Start()
    {
        elapsedTimeSecBuffer = 0f;

        messageController.OnActive
            .Subscribe(_ => Pause())
            .AddTo(this);

        messageController.OnInactive
            .Subscribe(isShowUIs => Resume(isShowUIs))
            .AddTo(this);
    }

    void Update()
    {
        // Avoid loss of trailing digits
        elapsedTimeSecBuffer += Time.deltaTime;
        ushort deltaSec = (ushort)elapsedTimeSecBuffer;

        // Add integer part of elapsed second
        elapsedTimeSecBuffer -= deltaSec;
        elapsedTimeSec += deltaSec;
    }

    public void Pause(bool isHideUIs = false)
    {
        if (isPaused) return;

        if (isHideUIs) input.SetInputVisible(false);
        Time.timeScale = 0f;

        isPaused = isScaled = true;
    }

    public void Resume(bool isShowUIs = true)
    {
        if (!isScaled) return;

        if (isShowUIs) input.SetInputVisible(true);
        Time.timeScale = 1f;

        isPaused = isScaled = false;
    }

    public void TimeScale(float scale = 5f)
    {
        if (isPaused) return;

        Time.timeScale = scale;

        isScaled = true;
    }
}