using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UniRx;
using System;

public class RestUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private RestButton restButton = default;
    [SerializeField] private Image invalidIcon = default;
    [SerializeField] private ResumeButton resumeButton = default;
    [SerializeField] private RestLifeGauge restLifeGauge = default;
    [SerializeField] private TextMeshProUGUI txtRest = default;
    [SerializeField] private CoverScreen cover = default;

    private Image image;
    private FadeTween fade;
    private Tween healTween;
    private Tween poisonTween;
    private Tween damageTween;
    private Tween cancelValidTween;

    private float healPoint = 0.005f;

    private ISubject<float> healSubject = new Subject<float>();
    public IObservable<float> Heal => healSubject;

    private float poisonPoint = 0.005f;

    private ISubject<float> poisonSubject = new Subject<float>();
    public IObservable<float> Poison => poisonSubject;

    public bool isActive { get; protected set; } = false;
    private bool isPoison = false;
    private bool isCold = false;

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData) => Inactivate();

    protected void Awake()
    {
        image = GetComponent<Image>();
        fade = new FadeTween(image);

        invalidIcon.gameObject.SetActive(false);

        healTween = DOTween.Sequence()
            .AppendCallback(() => healPoint += 0.0005f)
            .AppendCallback(() => healSubject.OnNext(healPoint))
            .AppendInterval(0.1f) // heal per 6frames
            .SetLoops(-1, LoopType.Restart)
            .SetUpdate(true)
            .AsReusable(gameObject);

        damageTween = DOTween.Sequence()
            .AppendCallback(() => fade.color = Color.white)
            .AppendCallback(() => fade.SetAlpha(1f))
            .AppendInterval(0.025f)
            .AppendCallback(() => fade.color = Color.red)
            .Append(fade.Out(0.8f, 0f, null, null, false))
            .SetUpdate(true)
            .AsReusable(gameObject);
    }

    protected void Start()
    {
        restButton.Click.Subscribe(_ => Activate());
        resumeButton.onPush.AddListener(Inactivate);

        HideUIs();
    }

    public void Activate()
    {
        if (isCold)
        {
            ActiveMessageController.Instance.InputMessageData("寝るな…！寝たら死ぬぞ…！", SDFaceID.SURPRISE, SDEmotionID.BLUE);
            return;
        }

        cover.FadeOutObservable(0.01f)
            .IgnoreElements()
            .Subscribe(null, ShowUIs)
            .AddTo(this);

        TimeManager.Instance.TimeScale();

        healSubject.OnNext(0f);
        healTween.Restart();

        isActive = true;

        if (isPoison)
        {
            poisonPoint = 0.005f;
            poisonTween.Pause();
        }
    }

    protected void ShowUIs()
    {
        image.raycastTarget = true;
        restLifeGauge.Enable();
        txtRest.gameObject.SetActive(true);
        resumeButton.Show().Play();

        restButton.interactable = false;
    }

    public void Inactivate()
    {
        TimeManager.Instance.TimeScale(1f);
        cover.FadeIn(0.01f);

        healPoint = 0.005f;
        healTween.Pause();

        HideUIs();

        isActive = false;

        if (isPoison)
        {
            poisonTween.Restart();
        }
    }

    protected void HideUIs()
    {
        image.raycastTarget = false;
        restLifeGauge.Disable();
        txtRest.gameObject.SetActive(false);
        resumeButton.Hide().Play();

        restButton.interactable = true;
    }

    public void OnLifeChange(float life, float lifeMax)
        => restLifeGauge.OnLifeChange(life, lifeMax);

    public void OnDamage()
    {
        Inactivate();
        damageTween.Restart();
    }

    public void SetPoison()
    {
        poisonPoint = 0.005f;

        poisonTween = DOTween.Sequence()
            .AppendCallback(() => poisonPoint += 0.0005f)
            .AppendCallback(() => poisonSubject.OnNext(poisonPoint))
            .AppendInterval(2f) // poison damage per 120frames
            .SetLoops(-1, LoopType.Restart)
            .SetUpdate(false)
            .AsReusable(gameObject)
            .Play();

        isPoison = true;
    }

    public void RemovePoison()
    {
        poisonTween?.Kill();
        isPoison = false;
    }

    public void SetCold()
    {
        isCold = true;
        invalidIcon.gameObject.SetActive(true);
    }

    public void RemoveCold()
    {
        isCold = false;
        invalidIcon.gameObject.SetActive(false);
    }
}
