using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ImageIcon : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmpCaption = default;

    private readonly Vector2 spaceCaption = new Vector2(0f, 40f);

    private Vector2 defaultPos;

    private Image image;
    private RectTransform rectTransform;

    public float ImageSpace => rectTransform.sizeDelta.x;

    public void SetEnabled(bool isEnable)
    {
        image.enabled = isEnable;
        tmpCaption.enabled = isEnable;
    }

    public void SetCaption(string caption = null)
    {
        tmpCaption.text = caption ?? "";
        rectTransform.anchoredPosition = defaultPos + (tmpCaption.text != "" ? spaceCaption : Vector2.zero);
    }

    void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        image.enabled = false;
        defaultPos = rectTransform.anchoredPosition;
    }

    public void SetSource(Sprite sprite, Material material)
    {
        image.sprite = sprite;
        image.material = material;
    }
}
