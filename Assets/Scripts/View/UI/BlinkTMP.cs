using UnityEngine;
using DG.Tweening;
using TMPro;

public class BlinkTMP : MonoBehaviour
{
    private TextMeshProUGUI txtMP;
    private Tween blinkTween = null;

    void Awake()
    {
        txtMP = GetComponent<TextMeshProUGUI>();

        blinkTween = DOTween
            .ToAlpha(() => txtMP.color, color => txtMP.color = color, 0f, 1f)
            .SetLoops(-1, LoopType.Restart)
            .AsReusable(gameObject);
    }

    public void BlinkStart()
    {
        blinkTween?.Restart();
    }

    public void BlinkStop()
    {
        blinkTween?.Pause();
    }

    public void Activate()
    {
        gameObject.SetActive(true);
        BlinkStart();
    }

    public void Inactivate()
    {
        blinkTween?.Pause();
        gameObject.SetActive(false);
    }
}
