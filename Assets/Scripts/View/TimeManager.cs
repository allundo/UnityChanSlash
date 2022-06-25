using UnityEngine;

public class TimeManager : SingletonMonoBehaviour<TimeManager>
{
    [SerializeField] private PlayerCommandTarget playerTarget = default;
    public ICommandTarget target;

    private IPlayerInput input;

    public bool isPaused { get; private set; } = false;
    public bool isScaled { get; private set; } = false;

    public double elapsedTimeSec { get; private set; } = 0;

    protected override void Awake()
    {
        base.Awake();
        target = playerTarget;
    }

    void Start()
    {
        input = target.input as IPlayerInput;
        elapsedTimeSec = 0;
    }

    void Update()
    {
        elapsedTimeSec += Time.deltaTime;
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