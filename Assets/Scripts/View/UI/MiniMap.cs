using UnityEngine;
using UnityEngine.UI;

public class MiniMap : MonoBehaviour
{
    private readonly int MINIMAP_SIZE = 15;

    private RawImage image = default;
    private RectTransform rectTransform;

    private RenderTexture renderTexture;

    private Vector2 defaultSize;

    private WorldMap map;

    void Awake()
    {
        map = GameManager.Instance.worldMap;

        image = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();

        defaultSize = rectTransform.sizeDelta;

        // Depth is set to 0 for current 2D map use
        image.texture = renderTexture = new RenderTexture((int)defaultSize.x, (int)defaultSize.y, 0);
    }

    public void UpdateMiniMap()
    {
        Graphics.Blit(map.GetMiniMap(MINIMAP_SIZE), renderTexture);
    }
}
