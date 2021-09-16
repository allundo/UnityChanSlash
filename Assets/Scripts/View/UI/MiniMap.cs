using UnityEngine;
using UnityEngine.UI;

public class MiniMap : MonoBehaviour
{
    private RawImage image = default;
    private RectTransform rectTransform;

    private RenderTexture renderTexture;

    private Vector2 defaultSize;

    void Awake()
    {
        image = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();

        defaultSize = rectTransform.sizeDelta;

        // Depth is set to 0 for current 2D map use
        image.texture = renderTexture = new RenderTexture((int)defaultSize.x, (int)defaultSize.y, 0);
    }

    private void Update()
    {
        Texture2D tex = GameManager.Instance.worldMap.GetMiniMap();
        Graphics.Blit(tex, renderTexture);
    }
}
