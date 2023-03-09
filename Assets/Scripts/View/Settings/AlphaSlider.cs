using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;

public class AlphaSlider : MonoBehaviour
{
    [SerializeField] private Slider slider = default;
    [SerializeField] private Toggle display = default;
    [SerializeField] private TextMeshProUGUI tmValue = default;
    [SerializeField] private TextMeshProUGUI tmType = default;
    [SerializeField] private ImageAlphaSetter[] images = default;

    public void SetTypeName(string type) => tmType.text = $"{type}:";
    public void SetAlpha(float alpha) => slider.value = Mathf.Clamp01(alpha) * slider.maxValue;
    public float GetAlpha() => slider.value / slider.maxValue;
    public void SetDisplay(bool isEnable) => display.isOn = isEnable;
    public void SetInteractable(bool isEnable) => slider.enabled = display.enabled = isEnable;

    void Start()
    {
        slider.OnValueChangedAsObservable()
            .Subscribe(value => ChangeValue(value))
            .AddTo(this);

        display.OnValueChangedAsObservable()
            .Subscribe(isEnable => DisplayImage(isEnable))
            .AddTo(this);
    }

    private void ChangeValue(float value)
    {
        if (tmValue != null) tmValue.text = $"{(int)value} %";
        images.ForEach(setter => setter.alpha = value / slider.maxValue);
    }

    private void DisplayImage(bool isEnable)
    {
        images.ForEach(setter => setter.SetDisplay(isEnable));
    }

}