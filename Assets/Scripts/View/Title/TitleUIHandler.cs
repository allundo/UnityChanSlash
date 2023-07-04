using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using UniRx;

public class TitleUIHandler : MonoBehaviour
{
    [SerializeField] private LogoAnimation logo = default;
    [SerializeField] private UnityChanIcon unityChanIcon = default;
    [SerializeField] private CameraWork cameraWork = default;
    [SerializeField] private TitleAnimation txtUnity = default;
    [SerializeField] private TitleAnimation txtSlash = default;
    [SerializeField] private SelectButtons selectButtons = default;
    [SerializeField] private TitleFaceAnimator unityChanAnim = default;
    [SerializeField] private FadeScreen fade = default;
    [SerializeField] public Button[] debugStart = default;
    [SerializeField] public Button[] debugEnding = default;
    [SerializeField] public Button[] debugResult = default;

    private Transform tfUnityChan;
    private AudioSource dropStart;

    public IObservable<object> TransitSignal;
    public IObservable<Unit> SettingsButtonSignal;
    public IObservable<Unit> ResultsButtonSignal;

    void Awake()
    {
        tfUnityChan = unityChanAnim.GetComponent<Transform>();
        dropStart = GameInfo.Instance.dropStart;

        TransitSignal =
            selectButtons.startButton
                .OnClickAsObservable()
                .ContinueWith(_ =>
                {
                    BGMManager.Instance.FadeOut(1f, true);
                    return StartSequence().OnCompleteAsObservable();
                });

        SettingsButtonSignal =
            selectButtons.settingsButton
                .OnClickAsObservable()
                .ContinueWith(button => ButtonSequence(button).OnCompleteAsObservable())
                .ContinueWith(_ =>
                {
                    BGMManager.Instance.FadeOut(1f, true);
                    return fade.FadeOutObservable(1f);
                });

        ResultsButtonSignal =
            selectButtons.resultsButton
                .OnClickAsObservable()
                .ContinueWith(button => ButtonSequence(button).OnCompleteAsObservable())
                .ContinueWith(_ =>
                {
                    BGMManager.Instance.SetDistance(0.75f, 1f);
                    return fade.FadeOutObservable(1f);
                });


        fade.SetAlpha(1f);

        // DEBUG ONLY
        debugStart.ForEach(btn => btn.gameObject.SetActive(Debug.isDebugBuild));
        debugEnding.ForEach(btn => btn.gameObject.SetActive(Debug.isDebugBuild));
        debugResult.ForEach(btn => btn.gameObject.SetActive(Debug.isDebugBuild));
    }

    public IObservable<Tween> Logo()
    {
        fade.FadeIn(1f);

        return
            DOTween.Sequence()
                .Join(unityChanIcon.LogoTween())
                .Join(logo.LogoTween())
                .OnCompleteAsObservable();
    }

    public void SkipLogo()
    {
        logo.Inactivate();
        ToTitle();
        fade.FadeIn(1f, 1f);
    }

    public void ToTitle()
    {
        BGMManager.Instance.PlayTitle();
        // camera work duration is 1.2f
        cameraWork.TitleTween().Play();
        if (Util.Judge(4)) DOVirtual.DelayedCall(0.95f, unityChanAnim.stagger.Fire).Play();

        unityChanIcon.TitleTween(selectButtons).Play();
        logo.TitleTween().Play();
        selectButtons.TitleTween().Play();
        txtUnity.TitleTween().Play();
        txtSlash.TitleTween().Play();
    }

    private Tween StartSequence()
    {
        Tween startTween = DOTween.Sequence()
            .AppendCallback(() => BGMManager.Instance.FadeOut(2f, true, Ease.OutQuad))
            .AppendCallback(() => selectButtons.startButton.PressedTween(16).Play())
            .Join(cameraWork.StartTween());

        Tween unityChanDropTween = tfUnityChan.DOMove(new Vector3(0f, -15f, 0), 2f)
            .OnPlay(() =>
            {
                cameraWork.StopCameraWork();
                cameraWork.ShakeTween().Play();
                unityChanAnim.drop.Fire();
            })
            .SetEase(Ease.InQuad)
            .SetRelative(true);

        Sequence fadeOutTween = DOTween.Sequence()
            .AppendCallback(cameraWork.StartTrail)
            .AppendCallback(() => fade.FadeOut(1.25f))
            .Join(txtUnity.CameraOutTween())
            .Join(txtSlash.CameraOutTween())
            .Join(selectButtons.CameraOutTween());

        return
            DOTween.Sequence()
                .Append(startTween)
                .AppendInterval(0.4f)
                .AppendCallback(() => dropStart.PlayEx())
                .Append(unityChanDropTween)
                .Join(fadeOutTween.SetDelay(0.75f))
                .AppendInterval(0.5f);
    }

    private Tween ButtonSequence(TwoPushButton button)
    {
        return DOTween.Sequence()
            .Append(button.PressedTween())
            .AppendCallback(() => fade.FadeOut(1f));
    }

    public IObservable<Unit> FadeOutObservable(float duration = 1f)
        => fade.FadeOutObservable(duration);
}
