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
    [SerializeField] private ResumeButton resumeButton = default;
    [SerializeField] private RestLifeGauge restLifeGauge = default;
    [SerializeField] private TextMeshProUGUI txtRest = default;
    [SerializeField] private CoverScreen cover = default;

    private Image image;
    private FadeTween fade;
    private Tween healTween;
    private Tween damageTween;
    private Tween cancelValidTween;

    private float healPoint = 0.0f;

    private ISubject<float> healSubject = new Subject<float>();
    public IObservable<float> Heal => healSubject;

    public bool isActive { get; protected set; } = false;

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData) => Inactivate();

    protected void Awake()
    {
        image = GetComponent<Image>();
        fade = new FadeTween(image);

        healTween = DOTween.Sequence()
            .AppendCallback(() => healPoint += 0.0001f)
            .AppendCallback(() => healSubject.OnNext(healPoint))
            .AppendInterval(0.05f)
            .SetLoops(-1, LoopType.Restart)
            .SetUpdate(true)
            .AsReusable(gameObject);

        // BUG:
        //   Disable rest canceling by background pointer up 0.5 sec from start resting, since
        //   unexpected OnPointerUp() is called when player is attacked immediately after start resting.
        cancelValidTween = DOVirtual
            .DelayedCall(0.5f, () => image.raycastTarget = true)
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
        // restButton.onClick.AddListener(Activate);
        restButton.Click.Subscribe(_ => Activate());
        resumeButton.onPush.AddListener(Inactivate);

        HideUIs();
    }

    public void Activate()
    {
        cover.FadeOut(0.01f).Play();
        GameManager.Instance.TimeScale();

        healSubject.OnNext(0f);
        healTween.Restart();

        ShowUIs();

        isActive = true;
    }

    protected void ShowUIs()
    {
        cancelValidTween.Restart();

        restLifeGauge.gameObject.SetActive(true);
        txtRest.gameObject.SetActive(true);
        resumeButton.Show().Play();

        restButton.interactable = false;
    }

    public void Inactivate()
    {
        GameManager.Instance.TimeScale(1f);
        cover.FadeIn(0.01f).Play();

        healPoint = 0f;
        healTween.Pause();

        HideUIs();

        isActive = false;
    }

    protected void HideUIs()
    {
        cancelValidTween.Pause();
        image.raycastTarget = false;
        restLifeGauge.gameObject.SetActive(false);
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
}
