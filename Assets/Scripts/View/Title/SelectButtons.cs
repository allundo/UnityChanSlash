using UnityEngine;
using UniRx;
using System;
using DG.Tweening;

public class SelectButtons : MonoBehaviour
{
    [SerializeField] public TwoPushButton startButton = default;
    [SerializeField] public TwoPushButton optionButton = default;
    [SerializeField] public TwoPushButton resultsButton = default;
    [SerializeField] private UnityChanIcon unityChanIcon = default;

    private RectTransform rt;
    private Vector2 defaultPos;

    private RectTransform rtStart;
    private RectTransform rtOption;
    private RectTransform rtResults;

    private TwoPushButton currentButton;

    void Awake()
    {
        rt = GetComponent<RectTransform>();

        rtStart = startButton.GetComponent<RectTransform>();
        rtOption = optionButton.GetComponent<RectTransform>();
        rtResults = resultsButton.GetComponent<RectTransform>();

        defaultPos = rt.anchoredPosition;
    }

    void Start()
    {
        startButton.Selected.Subscribe(button => SetCurrentButton(button)).AddTo(this);
        optionButton.Selected.Subscribe(button => SetCurrentButton(button)).AddTo(this);
        resultsButton.Selected.Subscribe(button => SetCurrentButton(button)).AddTo(this);

        unityChanIcon.FinishLogoTask.Subscribe(tf => SetIconAsChild(tf)).AddTo(this);
    }

    private void SetCurrentButton(TwoPushButton button)
    {
        currentButton?.Deselect();
        currentButton = button;
        unityChanIcon.SelectTween(button.IconPos);
    }

    private void ActivateButtons()
    {
        startButton.SetInteractable();
        optionButton.SetInteractable();
        resultsButton.SetInteractable();

        startButton.Select(true);
    }

    public void TitleTween()
    {
        rt.anchoredPosition = new Vector2(-2820f, defaultPos.y);

        DOTween.Sequence()
            .AppendInterval(0.4f)
            .Append(rt.DOAnchorPos(new Vector2(0f, defaultPos.y), 0.6f).SetEase(Ease.Linear))
            .Append(Overrun(100f, 0.5f))
            .OnComplete(ActivateButtons)
            .Play();
    }

    private Tween Overrun(float overrun, float duration)
    {
        return DOTween.Sequence()
            .Append(rt.DOAnchorPos(new Vector2(overrun, defaultPos.y), duration * 0.5f).SetEase(Ease.OutQuad))
            .Append(rt.DOAnchorPos(new Vector2(0f, defaultPos.y), duration * 0.5f).SetEase(Ease.InQuad));
    }

    public Tween CameraOutTween()
    {
        return rt.DOAnchorPos(new Vector2(0f, 1280f), 0.2f);
    }

    private void SetIconAsChild(Transform tfIcon)
    {
        tfIcon.SetParent(transform);
    }
}
