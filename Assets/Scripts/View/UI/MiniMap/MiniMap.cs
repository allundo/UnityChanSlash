using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MiniMap : MonoBehaviour
{
    [SerializeField] private UISymbolGenerator enemyPointGenerator = default;
    [SerializeField] private PlayerSymbol playerSymbol = default;

    private readonly int MINIMAP_SIZE = 15;
    private Vector2 uiTileUnit;

    private RawImage image = default;
    private RectTransform rectTransform;

    private RenderTexture renderTexture;

    private Vector2 defaultSize;

    private WorldMap map;

    private Vector3 distance;
    private Vector3 center;

    private bool isUpdated = false;

    private Dictionary<MobReactor, UISymbol> enemies = new Dictionary<MobReactor, UISymbol>();

    void Awake()
    {
        map = GameManager.Instance.worldMap;

        image = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();

        defaultSize = rectTransform.sizeDelta;
        uiTileUnit = defaultSize / MINIMAP_SIZE;

        // Depth is set to 0 for current 2D map use
        image.texture = renderTexture = new RenderTexture((int)defaultSize.x, (int)defaultSize.y, 0);

        // Use only local anchored position from parent for UI object
        enemyPointGenerator.spawnPoint = Vector3.zero;

        playerSymbol.SetSize(uiTileUnit);
    }

    void Update()
    {
        MoveEnemySymbols(center);
    }

    void LateUpdate()
    {
        isUpdated = false;
    }

    private void MoveEnemySymbols(Vector3 miniMapCenter)
    {
        if (isUpdated) return;

        enemies.ForEach(kv => kv.Value.SetPos(UIOffset(kv.Key.transform.position - miniMapCenter)));
    }

    private Vector2 UIOffset(Vector3 vec)
    {
        Vector3 tileUnitVec = vec / WorldMap.TILE_UNIT;
        return new Vector2(tileUnitVec.x * uiTileUnit.x, tileUnitVec.z * uiTileUnit.y);
    }

    private Vector2 UIOffsetDiscrete(Vector3 vec)
    {
        Vector3 tileUnitVec = vec / WorldMap.TILE_UNIT;
        return new Vector2(Mathf.Round(tileUnitVec.x) * uiTileUnit.x, Mathf.Round(tileUnitVec.z) * uiTileUnit.y);
    }

    public void UpdateMiniMap()
    {
        Graphics.Blit(map.GetMiniMap(MINIMAP_SIZE), renderTexture);

        center = map.MiniMapCenterWorldPos(MINIMAP_SIZE);
        playerSymbol.SetPos(UIOffsetDiscrete(GameManager.Instance.PlayerWorldPos - center));
        MoveEnemySymbols(center);
        isUpdated = true;
    }

    public PlayerSymbol Turn(IDirection dir) => playerSymbol.SetDir(dir);

    public void OnEnemyFind(Collider col)
    {
        var enemy = col.GetComponent<MobReactor>();

        if (enemy != null)
        {
            Vector2 uiOffset = UIOffset(enemy.transform.position - GameManager.Instance.PlayerWorldPos);
            enemies[enemy] = enemyPointGenerator.Spawn(uiOffset).SetSize(uiTileUnit);
        }
    }

    public void OnEnemyLeft(Collider col)
    {
        var enemy = col.GetComponent<MobReactor>();

        if (enemy != null)
        {
            enemies[enemy].Inactivate();
            enemies.Remove(enemy);
        }
    }
}
