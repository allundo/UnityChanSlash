using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Image))]
public class BGText : MonoBehaviour, ITextObject
{
    protected Image bgImage;

    [SerializeField] protected TextMeshProUGUI textMesh = default;

    protected virtual void Awake()
    {
        bgImage = GetComponent<Image>();
    }

    public string text
    {
        get { return textMesh.text; }
        set { textMesh.text = value; }
    }

    public void SetTextAlpha(float alpha = 1f)
    {
        textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, alpha);
    }

    public TextMeshProUGUI TextMesh => textMesh;

    public void SetActive(bool isActive) => gameObject.SetActive(isActive);
    public void SetTextEnable(bool isEnable) => textMesh.enabled = isEnable;
    public void SetBGEnable(bool isEnable) => bgImage.enabled = isEnable;
}
