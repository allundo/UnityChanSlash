using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using UniRx;

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

    [SerializeField] protected ParticleSystem[] stopArrowPrefabs = default;
    protected List<ParticleSystem> stopArrows = new List<ParticleSystem>();

    [SerializeField] protected ParticleSystem upArrowPrefab = default;
    [SerializeField] protected ParticleSystem downArrowPrefab = default;
    [SerializeField] protected ParticleSystem rightArrowPrefab = default;
    [SerializeField] protected ParticleSystem leftArrowPrefab = default;
    protected ParticleSystem upArrow = default;
    protected ParticleSystem downArrow = default;
    protected ParticleSystem rightArrow = default;
    protected ParticleSystem leftArrow = default;

    [SerializeField] private HandleButton handleButton = default;
    [SerializeField] private FlickInteraction flick = default;

    private bool isPressed = false;

    protected RectTransform rectTransform;
    protected Vector2 screenPos;

    public Vector2 Size => rectTransform.sizeDelta;

    void Awake()
    {
        InstantiateAllPrefabs();

        SetParentToTexts();
        SetParentToMoveArrows();
        SetParentToStopArrows();

        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        ResetCenterPos();

        InactiveTexts();
        SetActiveMoveArrows(false);
        SetActiveStopArrows(false);

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
        if (upTextPrefab != null) upText = Instantiate(upTextPrefab);
        if (downTextPrefab != null) downText = Instantiate(downTextPrefab);
        if (rightTextPrefab != null) rightText = Instantiate(rightTextPrefab);
        if (leftTextPrefab != null) leftText = Instantiate(leftTextPrefab);

        foreach (var prefab in stopArrowPrefabs)
        {
            stopArrows.Add(Instantiate(prefab));
        }

        if (upArrowPrefab != null) upArrow = Instantiate(upArrowPrefab);
        if (downArrowPrefab != null) downArrow = Instantiate(downArrowPrefab);
        if (rightArrowPrefab != null) rightArrow = Instantiate(rightArrowPrefab);
        if (leftArrowPrefab != null) leftArrow = Instantiate(leftArrowPrefab);
    }

    /// <summary>
    /// UGUI objects must be set as children of Canvas object
    /// </summary>
    public void SetParentToMoveArrows()
    {
        DoAllMoveArrow(SetParentToArrow);
    }

    public void SetParentToStopArrows()
    {
        foreach (var arrow in stopArrows)
        {
            SetParentToArrow(arrow);
        }
    }

    public void SetParentToArrow(ParticleSystem arrow)
    {
        arrow.transform.SetParent(transform);
        arrow.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    public void SetParentToTexts()
    {
        DoAllText(textMP =>
        {
            textMP.transform.SetParent(transform);
            textMP.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        });
    }

    private void OnDragUp(float dragRatio)
    {
        upText.gameObject.SetActive(dragRatio > 0.5f);

        SetActiveArrowsExceptFor(upArrow, dragRatio < 0.5f);

        OnDrag(dragRatio);
    }

    private void OnDragDown(float dragRatio)
    {
        downText.gameObject.SetActive(dragRatio > 0.5f);

        SetActiveArrowsExceptFor(downArrow, dragRatio < 0.5f);

        OnDrag(dragRatio);
    }

    private void OnDragRight(float dragRatio)
    {
        rightText.gameObject.SetActive(dragRatio > 0.5f);

        SetActiveArrowsExceptFor(rightArrow, dragRatio < 0.5f);

        OnDrag(dragRatio);
    }

    private void OnDragLeft(float dragRatio)
    {
        leftText.gameObject.SetActive(dragRatio > 0.5f);

        SetActiveArrowsExceptFor(leftArrow, dragRatio < 0.5f);

        OnDrag(dragRatio);
    }

    public void OnDrag(float dragRatio)
    {
        handleButton.OnDrag(dragRatio);

        if (isPressed) return;

        isPressed = true;

        SetActiveStopArrows(false);
        SetActiveMoveArrows(true);
    }

    public void OnRelease()
    {
        isPressed = false;

        handleButton.OnRelease();

        InactiveTexts();

        SetActiveStopArrows(true);
        SetActiveMoveArrows(false);
    }

    public void Activate(float alpha)
    {
        isPressed = false;

        gameObject.SetActive(true);

        handleButton.Activate(alpha);
        SetActiveStopArrows(true);
    }

    public void Inactivate()
    {
        OnRelease();
        gameObject.SetActive(false);

        handleButton.Inactivate();

        InactiveTexts();

        SetActiveStopArrows(false);
        SetActiveMoveArrows(false);
    }

    public void InactiveTexts()
    {
        DoAllText(textMP =>
        {
            textMP.gameObject.SetActive(false);
        });
    }

    public void SetAlpha(float alpha)
    {
        SetTextAlpha(alpha);
        handleButton.SetAlpha(alpha);
    }

    private void SetTextAlpha(float alpha)
    {
        DoAllText(textMP =>
        {
            Color c = textMP.color;
            textMP.color = new Color(c.r, c.g, c.b, alpha);
        });
    }

    private void DoAllText(Action<TextMeshProUGUI> action)
    {
        foreach (var textMP in new TextMeshProUGUI[] { upText, downText, rightText, leftText })
        {
            if (textMP == null) continue;
            action(textMP);
        }
    }

    public void SetActiveMoveArrows(bool isActivate)
    {
        DoAllMoveArrow(arrow => arrow.gameObject.SetActive(isActivate));
    }

    public void SetActiveStopArrows(bool isActivate)
    {
        DoAllStopArrow(arrow => arrow.gameObject.SetActive(isActivate));
    }

    private void DoAllMoveArrow(Action<ParticleSystem> action)
    {
        foreach (var arrow in new ParticleSystem[] { upArrow, downArrow, rightArrow, leftArrow })
        {
            if (arrow == null) continue;
            action(arrow);
        }
    }

    private void DoAllStopArrow(Action<ParticleSystem> action)
    {
        foreach (var arrow in stopArrows)
        {
            action(arrow);
        }
    }

    public void SetActiveArrowsExceptFor(ParticleSystem arrow, bool isActivate)
    {
        DoAllMoveArrow(activeArrow =>
        {
            activeArrow.gameObject.SetActive(activeArrow == arrow || isActivate);
        });
    }
}