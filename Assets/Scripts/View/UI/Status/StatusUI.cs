using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using System;
using DG.Tweening;

public class StatusUI : MonoBehaviour
{
    [SerializeField] private Button mapBtn = default;
    [SerializeField] private TextMeshProUGUI level = default;
    [SerializeField] private Image exp = default;
    [SerializeField] private TextMeshProUGUI attack = default;
    [SerializeField] private TextMeshProUGUI equipR = default;
    [SerializeField] private TextMeshProUGUI equipL = default;
    [SerializeField] private TextMeshProUGUI armor = default;
    [SerializeField] private TextMeshProUGUI shield = default;
    [SerializeField] private TextMeshProUGUI magic = default;
    [SerializeField] private TextMeshProUGUI resistFire = default;
    [SerializeField] private TextMeshProUGUI resistIce = default;
    [SerializeField] private TextMeshProUGUI resistDark = default;
    [SerializeField] private TextMeshProUGUI resistLight = default;

    private float expToNextLevel = 10000f;
    private RectTransform rectTransform;

    public IObservable<Unit> Switch => mapBtn.OnClickAsObservable();

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        Switch.Subscribe(_ => HideStatus()).AddTo(this);
    }

    public void UpdateValues(DispStatus status)
    {
        level.text = status.level.ToString();
        expToNextLevel = status.expToNextLevel;
        exp.fillAmount = status.exp / expToNextLevel;
        attack.text = status.attack.ToString();
        equipR.text = status.equipR.ToString();
        equipL.text = status.equipL.ToString();
        armor.text = status.armor.ToString();
        shield.text = status.shield.ToString();
        magic.text = status.magic.ToString();
        resistFire.text = status.resistFire.ToString();
        resistIce.text = status.resistIce.ToString();
        resistDark.text = status.resistDark.ToString();
        resistLight.text = status.resistLight.ToString();
    }

    public void UpdateExp(float exp)
    {
        this.exp.fillAmount = exp / expToNextLevel;
    }

    private void HideStatus(float duration = 0.25f)
    {
        mapBtn.enabled = false;
        mapBtn.gameObject.SetActive(false);
        HideTween(duration).OnComplete(() => gameObject.SetActive(false)).Play();
    }

    public void ShowStatus(float duration = 0.25f, float delay = 0.25f)
    {
        gameObject.SetActive(true);
        mapBtn.gameObject.SetActive(true);

        ShowTween(duration)
            .SetDelay(delay)
            .OnComplete(() => mapBtn.enabled = true)
            .Play();
    }

    protected Tween HideTween(float duration = 0.25f)
    {
        return rectTransform.DOAnchorPosX(540f, duration).SetRelative(true).SetEase(Ease.OutCubic);
    }

    protected Tween ShowTween(float duration = 0.25f)
    {
        return rectTransform.DOAnchorPosX(-540f, duration).SetRelative(true).SetEase(Ease.OutCubic);
    }
}