using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using DG.Tweening;
using UniRx;
using System;
using TMPro;

public class TwoPushButton : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private ParticleSystem prefSelectVfx = default;
    [SerializeField] private TextMeshProUGUI txtMP = default;

    private Button button;
    private bool isSelected = false;
    private bool isButtonValid = false;

    private UnityEvent onClickEvent = new UnityEvent();
    public UnityEvent onClick => onClickEvent;

    private ISubject<Unit> subject = new Subject<Unit>();
    public IObservable<Unit> OnClickAsObservable() => subject;

    private RectTransform rt;
    private Image image;
    private Vector2 defaultSize;
    private Color defaultColor;
    private float defaultRound;
    private Tween expandTween = null;
    private Tween shrinkTween = null;

    private float defaultTxtSize;
    private Color defaultTxtColor;
    private Tween expandTxtTween = null;
    private Tween shrinkTxtTween = null;

    private ParticleSystem selectVfx;

    private ISubject<TwoPushButton> selected = new Subject<TwoPushButton>();
    public IObservable<TwoPushButton> Selected => selected;

    public Vector2 Pos => rt.anchoredPosition;
    public Vector2 IconPos => Pos - new Vector2(defaultSize.x * 0.5625f, 0f);

    void Awake()
    {
        button = GetComponent<Button>();
        rt = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        selectVfx = GetInstance(prefSelectVfx);

        defaultSize = rt.sizeDelta;
        defaultColor = image.color;
        defaultRound = image.pixelsPerUnitMultiplier;
        defaultTxtSize = txtMP.fontSize;
        defaultTxtColor = txtMP.color;

        expandTween =
            DOTween.Sequence()
                .Join(Resize(1.25f, 1.1f, 0.05f))
                .Join(ResetColor(1f))
                .Join(ResetRound(0.9f))
                .AsReusable(gameObject);
        shrinkTween =
            DOTween.Sequence()
                .Join(Resize(1f, 1f))
                .Join(ResetColor(0.78f))
                .Join(ResetRound(1f))
                .AsReusable(gameObject)
                .Play();

        expandTxtTween =
            DOTween.Sequence()
                .Join(ResizeFont(1.2f))
                .Join(ResetTxtColor(1f))
                .AsReusable(gameObject);
        shrinkTxtTween =
            DOTween.Sequence()
                .Join(ResizeFont(1f, 0.1f))
                .Join(ResetTxtColor(0.78f))
                .AsReusable(gameObject)
                .Play();

        button.onClick.AddListener(Invoke);

        button.interactable = false;
    }

    private Tween Resize(float ratioX, float ratioY, float duration = 0.1f)
    {
        return rt.DOSizeDelta(new Vector2(defaultSize.x * ratioX, defaultSize.y * ratioY), duration);
    }

    private Tween ResetColor(float ratio, float duration = 0.1f)
    {
        return image.DOColor(defaultColor * ratio, duration);
    }

    private Tween ResetRound(float ratio, float duration = 0.1f)
    {
        return
            DOTween.To(
                () => image.pixelsPerUnitMultiplier,
                round => image.pixelsPerUnitMultiplier = round,
                defaultRound * ratio,
                duration
            );
    }

    private Tween ResizeFont(float ratio, float duration = 0.5f)
    {
        return
            DOTween.To(
                () => txtMP.fontSize,
                size => txtMP.fontSize = size,
                defaultTxtSize * ratio,
                duration
            );
    }

    private Tween ResetTxtColor(float ratio, float duration = 0.1f)
    {
        return txtMP.DOColor(defaultTxtColor * ratio, duration);
    }

    public Tween PressedTween()
    {
        return image.DOColor(new Color(1, 1, 1, 1), 0.1f).SetLoops(10, LoopType.Yoyo);
    }

    private T GetInstance<T>(T prefab)
        where T : UnityEngine.Object
    {
        T instance = Instantiate(prefab);
        Component component = instance as Component;
        component.transform.SetParent(transform.parent);
        component.transform.SetSiblingIndex(0);
        component.GetComponent<RectTransform>().anchoredPosition = rt.anchoredPosition;

        return instance;
    }

    private void Invoke()
    {
        if (isButtonValid)
        {
            onClickEvent.Invoke();
            subject.OnNext(Unit.Default);
        }
    }

    public void SetInteractable(bool isInteractable = true)
    {
        button.interactable = isInteractable;
    }

    public void Deselect(BaseEventData eventData = null)
    {
        isSelected = isButtonValid = false;
        ExecuteEvents.Execute<IDeselectHandler>(button.gameObject, eventData, (target, data) => target.OnDeselect(eventData));
        expandTween?.Pause();
        expandTxtTween?.Pause();
        shrinkTween?.Restart();
        shrinkTxtTween?.Restart();
        selectVfx?.Stop();
        selectVfx?.Clear();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Select();
    }

    public void Select(bool isButtonValid = false)
    {
        if (isSelected || !button.IsInteractable()) return;

        this.isButtonValid = isButtonValid;
        isSelected = true;
        button.Select();
        shrinkTween?.Pause();
        shrinkTxtTween?.Pause();
        expandTween?.Restart();
        expandTxtTween?.Restart();
        selectVfx?.Play();
        selected.OnNext(this);
    }

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isSelected) isButtonValid = true;
    }
}
