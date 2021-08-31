﻿using UnityEngine;
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
    [SerializeField] private FaceAnimator unityChanAnim = default;
    [SerializeField] private FadeScreen fade = default;

    private Transform tfUnityChan;

    public IObservable<object> TransitSignal;

    void Awake()
    {
        tfUnityChan = unityChanAnim.GetComponent<Transform>();

        TransitSignal =
            selectButtons.startButton
                .OnClickAsObservable()
                .SelectMany(_ => StartSequence().OnCompleteAsObservable());

        fade.SetAlpha(1f);
    }

    public IObservable<Tween> Logo()
    {
        return
            DOTween.Sequence()
                .Join(fade.FadeIn(1f))
                .Join(unityChanIcon.LogoTween())
                .Join(logo.LogoTween())
                .OnCompleteAsObservable();
    }

    public void SkipLogo()
    {
        logo.Inactivate();
        ToTitle();
        fade.FadeIn(1f, 1f).Play();
    }

    public void ToTitle()
    {
        cameraWork.TitleTween().Play();
        unityChanIcon.TitleTween(selectButtons).Play();
        logo.TitleTween().Play();
        selectButtons.TitleTween().Play();
        txtUnity.TitleTween().Play();
        txtSlash.TitleTween().Play();
    }

    private Tween StartSequence()
    {
        Tween startTween = DOTween.Sequence()
            .Join(selectButtons.startButton.PressedTween())
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
            .Join(fade.FadeOut(1.25f))
            .Join(txtUnity.CameraOutTween())
            .Join(txtSlash.CameraOutTween())
            .Join(selectButtons.CameraOutTween());

        return
            DOTween.Sequence()
                .Append(startTween)
                .AppendInterval(0.4f)
                .Append(unityChanDropTween)
                .Join(fadeOutTween.SetDelay(0.75f))
                .AppendInterval(0.5f);
    }
}
