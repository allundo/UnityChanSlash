﻿using UnityEngine;
using UniRx;

public class UIPosition : MonoBehaviour
{
    [SerializeField] private RectTransform rtUI = default;
    [SerializeField] private RectTransform rtLifeGauge = default;
    [SerializeField] private FightCircle fightCircle = default;
    [SerializeField] private float portraitFromBottom = 720f;
    [SerializeField] private float landscapeFromLeft = 480f;
    [SerializeField] private float lifeGaugeFromBottom = 60f;
    [SerializeField] private Vector2 lifeGaugeFromLeftTop = new Vector2(420f, -100f);

    public void ResetPosition(DeviceOrientation orientation)
    {
        switch (orientation)
        {
            case DeviceOrientation.Portrait:
                rtUI.anchoredPosition = new Vector2(0f, portraitFromBottom - Screen.height * (0.5f - ThirdPersonCamera.Margin));
                rtLifeGauge.anchoredPosition = new Vector2(0f, lifeGaugeFromBottom - Screen.height * 0.5f);
                break;

            case DeviceOrientation.LandscapeRight:
                rtUI.anchoredPosition = new Vector2(landscapeFromLeft - Screen.width * 0.5f, 0f);
                rtLifeGauge.anchoredPosition = lifeGaugeFromLeftTop + new Vector2(-Screen.width * 0.5f, Screen.height * 0.5f);
                break;
        }

        fightCircle.ResetCenterPos();
    }
}
