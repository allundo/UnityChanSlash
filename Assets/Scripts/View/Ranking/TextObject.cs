using TMPro;

public interface ITextObject
{
    string text { get; set; }
    void SetActive(bool isActive);
}

public class TextObject : ITextObject
{
    protected TextMeshProUGUI textMesh;
    public TextObject(TextMeshProUGUI textMesh)
    {
        this.textMesh = textMesh;
    }

    public string text
    {
        get { return textMesh.text; }
        set { textMesh.text = value; }
    }

    public void SetActive(bool isActive)
    {
        textMesh.gameObject.SetActive(isActive);
    }
}
