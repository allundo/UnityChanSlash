using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;
using System;
using DG.Tweening;

public class RankingUIHandler : MonoBehaviour
{
    [SerializeField] private FadeScreen fade = default;
    [SerializeField] private RecordsUI infoUI = default;
    [SerializeField] private RecordsUI deadRankUI = default;
    [SerializeField] private RecordsUI clearRankUI = default;
    [SerializeField] private Button toTitleBtn = default;
    [SerializeField] private Button rightBtn = default;
    [SerializeField] private Button leftBtn = default;

    private Button[] buttons;
    private void SetInteractableBtns(bool isInteractable) => buttons.ForEach(btn => { btn.interactable = isInteractable; });
    public IObservable<object> TransitSignal;

    private RecordsUI leftUI;
    private RecordsUI centerUI;
    private RecordsUI rightUI;

    private float width = 1080f;

    private string currentDisplay;
    private TextMeshProUGUI leftLabel;
    private TextMeshProUGUI rightLabel;

    void Awake()
    {
        TransitSignal = toTitleBtn.OnClickAsObservable().First() // ContinueWith() cannot handle duplicated click events
                .ContinueWith(_ => fade.FadeOut(2f).OnCompleteAsObservable());

        buttons = new Button[] { toTitleBtn, rightBtn, leftBtn };
        SetInteractableBtns(false);

        leftUI = deadRankUI;
        centerUI = infoUI;
        rightUI = clearRankUI;

        leftLabel = leftBtn.GetComponentInChildren<TextMeshProUGUI>();
        rightLabel = rightBtn.GetComponentInChildren<TextMeshProUGUI>();

        currentDisplay = "プレイ記録";
        leftLabel.text = "死亡録";
        rightLabel.text = "クリア録";
    }

    void Start()
    {
        rightBtn.OnClickAsObservable()
            .Subscribe(_ => GoToRight().Play())
            .AddTo(this);

        leftBtn.OnClickAsObservable()
            .Subscribe(_ => GoToLeft().Play())
            .AddTo(this);
    }

    public void ViewInfo()
    {
        fade.color = Color.black;
        fade.FadeIn(2f).OnComplete(() => SetInteractableBtns(true)).Play();
        centerUI.DisplayRecords();
    }

    private Tween GoToRight()
    {
        return DOTween.Sequence()
            .AppendCallback(() => SetInteractableBtns(false))
            .Join(rightUI.MoveX(-width, 0.6f).SetEase(Ease.OutCubic))
            .Join(centerUI.MoveX(-width, 0.6f).SetEase(Ease.Linear))
            .Join(leftUI.MoveX(-width, 0.6f).SetEase(Ease.OutCubic))
            .OnComplete(() =>
            {
                leftUI.SetPosX(width);

                var tmpUI = leftUI;
                leftUI = centerUI;
                centerUI = rightUI;
                rightUI = tmpUI;

                var tmpLabel = leftLabel.text;
                leftLabel.text = currentDisplay;
                currentDisplay = rightLabel.text;
                rightLabel.text = tmpLabel;

                centerUI.DisplayRecords();
                leftUI.HideRecords();
                rightUI.HideRecords();

                SetInteractableBtns(true);
            });
    }

    private Tween GoToLeft()
    {
        return DOTween.Sequence()
            .AppendCallback(() => SetInteractableBtns(false))
            .Join(rightUI.MoveX(width, 0.6f).SetEase(Ease.OutCubic))
            .Join(centerUI.MoveX(width, 0.6f).SetEase(Ease.Linear))
            .Join(leftUI.MoveX(width, 0.6f).SetEase(Ease.OutCubic))
            .OnComplete(() =>
            {
                rightUI.SetPosX(-width);

                var tmpUI = rightUI;
                rightUI = centerUI;
                centerUI = leftUI;
                leftUI = tmpUI;

                var tmpLabel = rightLabel.text;
                rightLabel.text = currentDisplay;
                currentDisplay = leftLabel.text;
                leftLabel.text = tmpLabel;

                centerUI.DisplayRecords();
                leftUI.HideRecords();
                rightUI.HideRecords();

                SetInteractableBtns(true);
            });
    }
}