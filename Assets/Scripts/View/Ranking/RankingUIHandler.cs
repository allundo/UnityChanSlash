using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;
using System;
using DG.Tweening;

public class RankingUIHandler : MonoBehaviour
{
    [SerializeField] private FadeScreen fade = default;
    [SerializeField] private Image infoUI = default;
    [SerializeField] private Image deadRankUI = default;
    [SerializeField] private Image clearRankUI = default;
    [SerializeField] private Button toTitleBtn = default;
    [SerializeField] private Button rightBtn = default;
    [SerializeField] private Button leftBtn = default;

    private Button[] buttons;
    private void SetEnableBtns(bool isEnable) => buttons.ForEach(btn => btn.enabled = isEnable);
    public IObservable<object> TransitSignal;

    private UITween leftUI;
    private UITween centerUI;
    private UITween rightUI;

    private float width;

    private string currentDisplay;
    private TextMeshProUGUI leftLabel;
    private TextMeshProUGUI rightLabel;

    void Awake()
    {
        TransitSignal = toTitleBtn.OnClickAsObservable().First() // ContinueWith() cannot handle duplicated click events
                .ContinueWith(_ => fade.FadeOut(2f).OnCompleteAsObservable());

        buttons = new Button[] { toTitleBtn, rightBtn, leftBtn };
        SetEnableBtns(false);

        width = Screen.width;

        leftUI = new UITween(deadRankUI.gameObject);
        centerUI = new UITween(infoUI.gameObject);
        rightUI = new UITween(clearRankUI.gameObject);

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
        fade.FadeIn(2f).OnComplete(() => SetEnableBtns(true)).Play();
    }

    private Tween GoToRight()
    {
        return DOTween.Sequence()
            .AppendCallback(() => SetEnableBtns(false))
            .Join(rightUI.MoveX(-width, 1f).SetEase(Ease.OutCubic))
            .Join(centerUI.MoveX(-width, 1f).SetEase(Ease.OutCubic))
            .Join(leftUI.MoveX(-width, 1f).SetEase(Ease.OutCubic))
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

                SetEnableBtns(true);
            });
    }

    private Tween GoToLeft()
    {
        return DOTween.Sequence()
            .AppendCallback(() => SetEnableBtns(false))
            .Join(rightUI.MoveX(width, 1f).SetEase(Ease.OutCubic))
            .Join(centerUI.MoveX(width, 1f).SetEase(Ease.OutCubic))
            .Join(leftUI.MoveX(width, 1f).SetEase(Ease.OutCubic))
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

                SetEnableBtns(true);
            });
    }
}