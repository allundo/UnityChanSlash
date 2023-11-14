using UnityEngine;

public class MessageWindowUI : MessageWindowBase
{
    [SerializeField] private Sprite picture = default;

    private Sprite window;
    private Vector2 windowSize;
    private Vector2 pictureSize;

    protected override void Awake()
    {
        base.Awake();
        window = fade.sprite;
        windowSize = uiTween.defaultSize;
        pictureSize = new Vector2(1024, 768);
    }

    public void SetPicture()
    {
        fade.sprite = picture;
        uiTween.SetSize(pictureSize, true);
        uiTween.ResetSizeY(0);

    }
    public void ResetToWindow()
    {
        fade.sprite = window;
        uiTween.SetSize(windowSize, true);
        uiTween.ResetSizeY(0);
    }
}