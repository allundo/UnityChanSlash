using UnityEngine;
using UniRx;
using System;
using DG.Tweening;

public class EndingUIHandler : MonoBehaviour
{
    [SerializeField] public EndingScreen screen = default;
    [SerializeField] public FadeScreen fade = default;

    public string periodType => screen.msgSource.name;

    public IObservable<Unit> StartScroll(int periodIndex = 0)
    {
        fade.color = Color.white;
        fade.FadeIn(3f, 0, false);

        return DOTween.Sequence()
            .AppendInterval(3f)
            .AppendCallback(() => fade.color = new Color(0, 0, 0, 0))
            .Append(screen.TextScrollSequence(periodIndex))
            .AppendCallback(() => fade.FadeOut(3f, 0, false))
            .AppendInterval(3f)
            .SetUpdate(false)
            .OnCompleteAsObservable(Unit.Default);
    }
}
