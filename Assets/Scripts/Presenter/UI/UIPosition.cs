using UnityEngine;

public class UIPosition : MonoBehaviour
{
    [SerializeField] private RectTransform rtUI = default;
    [SerializeField] private RectTransform rtLifeGauge = default;
    [SerializeField] private FightCircle fightCircle = default;
    [SerializeField] private MiniMap miniMap = default;
    [SerializeField] private RectTransform rtItemList = default;
    [SerializeField] private RestButton restButton = default;
    [SerializeField] private float portraitFromBottom = 720f;
    [SerializeField] private float landscapeFromLeft = 480f;
    [SerializeField] private float lifeGaugeFromBottom = 60f;
    [SerializeField] private Vector2 lifeGaugeFromLeftTop = new Vector2(420f, -100f);

    private Vector2 itemListSize;

    void Awake()
    {
        itemListSize = rtItemList.sizeDelta;
    }

    public void ResetOrientation(DeviceOrientation orientation)
    {
        switch (orientation)
        {
            case DeviceOrientation.Portrait:
                rtUI.anchoredPosition = new Vector2(0f, portraitFromBottom - Screen.height * (0.5f - ThirdPersonCamera.Margin));

                rtLifeGauge.anchoredPosition = new Vector2(0f, lifeGaugeFromBottom - Screen.height * 0.5f);

                rtItemList.anchorMin = rtItemList.anchorMax = new Vector2(0f, 0.5f);
                rtItemList.anchoredPosition = new Vector2(itemListSize.x, itemListSize.y + 280f) * 0.5f;
                break;

            case DeviceOrientation.LandscapeRight:
                rtUI.anchoredPosition = new Vector2(landscapeFromLeft - Screen.width * 0.5f, 0f);

                rtLifeGauge.anchoredPosition = lifeGaugeFromLeftTop + new Vector2(-Screen.width * 0.5f, Screen.height * 0.5f);

                rtItemList.anchorMin = rtItemList.anchorMax = new Vector2(1f, 0f);
                rtItemList.anchoredPosition = new Vector2(-itemListSize.x, itemListSize.y) * 0.5f;
                break;
        }

        miniMap.ResetOrientation(orientation);
        restButton.ResetOrientation(orientation);
        fightCircle.ResetCenterPos();
    }
}
