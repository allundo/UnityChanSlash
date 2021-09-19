using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UniRx;
using Coffee.UIExtensions;

public class DoorUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI upTextPrefab = default;
    [SerializeField] private TextMeshProUGUI downTextPrefab = default;
    [SerializeField] private TextMeshProUGUI rightTextPrefab = default;
    [SerializeField] private TextMeshProUGUI leftTextPrefab = default;

    public TextMeshProUGUI upText { get; protected set; } = default;
    public TextMeshProUGUI downText { get; protected set; } = default;
    public TextMeshProUGUI rightText { get; protected set; } = default;
    public TextMeshProUGUI leftText { get; protected set; } = default;
    protected TextMeshProUGUI[] texts;

    [SerializeField] protected UIParticle[] stopArrowPrefabs = default;
    protected UIParticle[] stopArrows;

    [SerializeField] protected UIParticle upArrowPrefab = default;
    [SerializeField] protected UIParticle downArrowPrefab = default;
    [SerializeField] protected UIParticle rightArrowPrefab = default;
    [SerializeField] protected UIParticle leftArrowPrefab = default;
    protected UIParticle upArrow = default;
    protected UIParticle downArrow = default;
    protected UIParticle rightArrow = default;
    protected UIParticle leftArrow = default;
    protected UIParticle[] moveArrows;

    [SerializeField] private HandleButton handleButton = default;
    [SerializeField] private FlickInteraction flick = default;

    private bool isPressed = false;

    protected RectTransform rectTransform;
    protected Vector2 screenPos;

    public Vector2 Size => rectTransform.sizeDelta;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        InstantiateAllPrefabs();

        SetUIsActive(texts, false);
        SetUIsActive(stopArrows, false);
        SetUIsActive(moveArrows, false);
    }

    void Start()
    {
        ResetCenterPos();

        if (upText != null && upArrow != null) flick.DragUp.SkipLatestValueOnSubscribe().Subscribe(ratio => OnDragUp(ratio)).AddTo(this);
        if (downText != null && downArrow != null) flick.DragDown.SkipLatestValueOnSubscribe().Subscribe(ratio => OnDragDown(ratio)).AddTo(this);
        if (rightText != null && rightArrow != null) flick.DragRight.SkipLatestValueOnSubscribe().Subscribe(ratio => OnDragRight(ratio)).AddTo(this);
        if (leftText != null && leftArrow != null) flick.DragLeft.SkipLatestValueOnSubscribe().Subscribe(ratio => OnDragLeft(ratio)).AddTo(this);

        flick.ReleaseSubject.Subscribe(_ => OnRelease()).AddTo(this);
    }

    public void ResetCenterPos()
    {
        screenPos = rectTransform.GetScreenPos();
    }

    public bool InRegion(Vector2 screenPos)
    {
        var vec = screenPos - this.screenPos;
        return Mathf.Abs(vec.x) < Size.x * 0.5f && Mathf.Abs(vec.y) < Size.y * 0.5f;
    }

    private void InstantiateAllPrefabs()
    {
        if (upTextPrefab != null) upText = InstantiateUI(upTextPrefab);
        if (downTextPrefab != null) downText = InstantiateUI(downTextPrefab);
        if (rightTextPrefab != null) rightText = InstantiateUI(rightTextPrefab);
        if (leftTextPrefab != null) leftText = InstantiateUI(leftTextPrefab);

        texts = new[] { upText, downText, rightText, leftText };

        stopArrows = stopArrowPrefabs.Select(arrow => InstantiateUI(arrow)).ToArray();

        if (upArrowPrefab != null) upArrow = InstantiateUI(upArrowPrefab);
        if (downArrowPrefab != null) downArrow = InstantiateUI(downArrowPrefab);
        if (rightArrowPrefab != null) rightArrow = InstantiateUI(rightArrowPrefab);
        if (leftArrowPrefab != null) leftArrow = InstantiateUI(leftArrowPrefab);

        moveArrows = new[] { upArrow, downArrow, rightArrow, leftArrow };
    }

    /// <summary>
    /// UGUI objects must be set as children of Canvas object
    /// </summary>
    private T InstantiateUI<T>(T prefabUI) where T : UnityEngine.Object
        => Instantiate(prefabUI, transform, false);

    private void SetUIsActive(MaskableGraphic[] sequence, bool isActive)
    {
        sequence.ForEach(ui => ui.gameObject.SetActive(isActive), null);
    }
    private void SetUIsActive(MaskableGraphic[] sequence, bool isActive, MaskableGraphic exceptFor)
    {
        sequence.ForEach(ui => ui.gameObject.SetActive(isActive), null, exceptFor);
    }

    private void OnDragUp(float dragRatio)
    {
        upText.gameObject.SetActive(dragRatio > 0.5f);

        SetUIsActive(moveArrows, dragRatio < 0.5f, upArrow);

        OnDrag(dragRatio);
    }

    private void OnDragDown(float dragRatio)
    {
        downText.gameObject.SetActive(dragRatio > 0.5f);

        SetUIsActive(moveArrows, dragRatio < 0.5f, downArrow);

        OnDrag(dragRatio);
    }

    private void OnDragRight(float dragRatio)
    {
        rightText.gameObject.SetActive(dragRatio > 0.5f);

        SetUIsActive(moveArrows, dragRatio < 0.5f, rightArrow);

        OnDrag(dragRatio);
    }

    private void OnDragLeft(float dragRatio)
    {
        leftText.gameObject.SetActive(dragRatio > 0.5f);

        SetUIsActive(moveArrows, dragRatio < 0.5f, leftArrow);

        OnDrag(dragRatio);
    }

    public void OnDrag(float dragRatio)
    {
        handleButton.OnDrag(dragRatio);

        if (isPressed) return;

        isPressed = true;

        SetUIsActive(stopArrows, false);
        SetUIsActive(moveArrows, true);
    }

    public void OnRelease()
    {
        isPressed = false;

        handleButton.OnRelease();

        SetUIsActive(texts, false);
        SetUIsActive(stopArrows, true);
        SetUIsActive(moveArrows, false);
    }

    public void Activate(float alpha)
    {
        isPressed = false;

        gameObject.SetActive(true);

        handleButton.Activate(alpha);
        SetUIsActive(stopArrows, true);
    }

    public void Inactivate()
    {
        OnRelease();
        gameObject.SetActive(false);

        handleButton.Inactivate();

        SetUIsActive(texts, false);
        SetUIsActive(stopArrows, false);
        SetUIsActive(moveArrows, false);
    }

    public void SetAlpha(float alpha)
    {
        SetTextAlpha(alpha);
        handleButton.SetAlpha(alpha);
    }

    private void SetTextAlpha(float alpha)
    {
        texts.ForEach(txt =>
        {
            Color c = txt.color;
            txt.color = new Color(c.r, c.g, c.b, alpha);
        }, null);
    }
}
