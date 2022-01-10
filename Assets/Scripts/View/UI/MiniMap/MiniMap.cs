using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MiniMap : MonoBehaviour
{
    [SerializeField] private UISymbolGenerator enemyPointGenerator = default;
    [SerializeField] private PlayerSymbol playerSymbol = default;
    [SerializeField] private Vector2 landscapeSize = new Vector2(420f, 420f);
    [SerializeField] private Vector2 portraitSize = new Vector2(480f, 480f);

    private static readonly int MINIMAP_SIZE = 15;
    private Vector2 uiTileUnit;

    private RawImage image = default;
    private RectTransform rectTransform;

    private RenderTexture renderTexture;

    private Vector2 defaultSize;
    private static readonly Vector2 OUT_OF_SCREEN = new Vector2(1024f, 1024f);

    private WorldMap map;

    private Vector3 center;

    private bool isUpdated = false;

    private Dictionary<EnemyReactor, UISymbol> enemies = new Dictionary<EnemyReactor, UISymbol>();

    private bool[,] tileViewOpen = new bool[MINIMAP_SIZE, MINIMAP_SIZE];

    void Awake()
    {
        map = GameManager.Instance.worldMap;

        image = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();

        // Depth is set to 0 for current 2D map use
        image.texture = renderTexture = new RenderTexture((int)portraitSize.x, (int)portraitSize.y, 0);

        // Use only local anchored position from parent for UI object
        enemyPointGenerator.spawnPoint = Vector3.zero;
    }

    public void ResetOrientation(DeviceOrientation orientation)
    {

        switch (orientation)
        {
            case DeviceOrientation.Portrait:
                rectTransform.sizeDelta = portraitSize;
                rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(1f, 0.5f);
                rectTransform.anchoredPosition = new Vector2(-(portraitSize.x + 40f), portraitSize.y + 280f) * 0.5f;
                break;

            case DeviceOrientation.LandscapeRight:
                rectTransform.sizeDelta = landscapeSize;
                rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(1f, 1f);
                rectTransform.anchoredPosition = new Vector2(-(landscapeSize.x + 40f), -landscapeSize.y) * 0.5f;
                break;
        }

        uiTileUnit = rectTransform.sizeDelta / MINIMAP_SIZE;
        playerSymbol.SetSize(uiTileUnit);
    }

    public void SwitchWorldMap(WorldMap map)
    {
        this.map = map;
        enemies.ForEach(kv => kv.Value.Inactivate());
        enemies.Clear();
    }

    void Update()
    {
        MoveEnemySymbols();
    }

    void LateUpdate()
    {
        isUpdated = false;
    }

    private void MoveEnemySymbols()
    {
        if (isUpdated) return;

        enemies.ForEach(kv => kv.Value.SetPos(UIOffset(kv.Key.transform.position)));
    }

    private Vector2 UIOffset(Vector3 worldPos)
    {
        if (!map.IsCurrentViewOpen(worldPos)) return OUT_OF_SCREEN;

        Vector3 tileUnitVec = (worldPos - center) / WorldMap.TILE_UNIT;
        return new Vector2(tileUnitVec.x * uiTileUnit.x, tileUnitVec.z * uiTileUnit.y);

    }

    private Vector2 UIOffsetDiscrete(Vector3 worldPos)
    {
        Vector3 tileUnitVec = (worldPos - center) / WorldMap.TILE_UNIT;
        return new Vector2(Mathf.Round(tileUnitVec.x) * uiTileUnit.x, Mathf.Round(tileUnitVec.z) * uiTileUnit.y);
    }

    public void UpdateMiniMap()
    {
        Graphics.Blit(map.GetMiniMap(MINIMAP_SIZE), renderTexture);

        center = map.MiniMapCenterWorldPos(MINIMAP_SIZE);
        playerSymbol.SetPos(UIOffsetDiscrete(GameManager.Instance.PlayerWorldPos));
        MoveEnemySymbols();
        isUpdated = true;
    }

    public PlayerSymbol Turn(IDirection dir) => playerSymbol.SetDir(dir);

    // Called as an event of enemy detecting Collider:Enter on Player
    public void OnEnemyFind(Collider col)
    {
        var enemy = col.GetComponent<EnemyReactor>();

        if (enemy != null && !enemies.ContainsKey(enemy))
        {
            Vector2 uiOffset = UIOffset(enemy.transform.position);
            enemies[enemy] = enemyPointGenerator.Spawn(uiOffset).SetSize(uiTileUnit);
        }
    }

    // Called as an event of enemy detecting Collider:Exit on Player
    public void OnEnemyLeft(Collider col)
    {
        var enemy = col.GetComponent<EnemyReactor>();

        if (enemy != null)
        {
            enemies[enemy].Inactivate();
            enemies.Remove(enemy);
        }
    }
}
