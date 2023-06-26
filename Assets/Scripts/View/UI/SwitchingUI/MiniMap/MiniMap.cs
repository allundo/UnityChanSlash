using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MiniMap : SwitchingContentBase
{
    [SerializeField] private UISymbolGenerator enemyPointGenerator = default;
    [SerializeField] private PlayerSymbol playerSymbol = default;

    private static readonly int MINIMAP_SIZE = 15;
    private static readonly int EXPAND_MAP_SIZE = 41;
    private Vector2 uiTileUnit;
    private int currentMiniMapSize = MINIMAP_SIZE;

    private RawImage image;

    private RenderTexture renderTexture;

    private static readonly Vector2 OUT_OF_SCREEN = new Vector2(1024f, 1024f);

    private MiniMapData mapData;

    private Vector3 center;

    private bool isUpdated = false;

    private Dictionary<EnemyReactor, UISymbol> enemies = new Dictionary<EnemyReactor, UISymbol>();

    protected override void Awake()
    {
        base.Awake();

        mapData = GameInfo.Instance.Map(0).miniMapData;
        isShown = true;
    }

    public override void InitUISize(float landscapeSize, float portraitSize, float expandSize)
    {
        base.InitUISize(landscapeSize, portraitSize, expandSize);

        image = GetComponent<RawImage>();

        // Depth is set to 0 for current 2D map use
        image.texture = renderTexture = new RenderTexture((int)portraitSize, (int)portraitSize, 0);
    }

    public override void ResetOrientation(DeviceOrientation orientation)
    {
        base.ResetOrientation(orientation);

        ResetUISize(currentSize, currentMiniMapSize, image.color.a);
        DrawMap();
    }

    public void SwitchWorldMap(MiniMapData mapData)
    {
        this.mapData = mapData;
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
        if (!mapData.IsCurrentViewOpen(worldPos)) return OUT_OF_SCREEN;

        Vector3 tileUnitVec = (worldPos - center) / WorldMap.TILE_UNIT;
        return new Vector2(tileUnitVec.x * uiTileUnit.x, tileUnitVec.z * uiTileUnit.y);
    }

    private Vector2 UIOffsetDiscrete(Vector3 worldPos)
    {
        Vector3 tileUnitVec = (worldPos - center) / WorldMap.TILE_UNIT;
        return new Vector2(Mathf.Round(tileUnitVec.x) * uiTileUnit.x, Mathf.Round(tileUnitVec.z) * uiTileUnit.y);
    }

    public void UpdateMiniMap(Vector3 playerPos) => UpdateMiniMap(playerPos, MINIMAP_SIZE);

    protected void UpdateMiniMap(Vector3 playerPos, int miniMapSize)
    {
        DrawMap(miniMapSize);

        center = mapData.MiniMapCenterWorldPos(miniMapSize);
        playerSymbol.SetPos(UIOffsetDiscrete(playerPos));
        MoveEnemySymbols();
        isUpdated = true;
    }

    protected void DrawMap() => DrawMap(currentMiniMapSize);
    protected void DrawMap(int miniMapSize) => Graphics.Blit(mapData.GetMiniMap(miniMapSize), renderTexture);

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

    public override void SetEnable(bool isEnabled)
    {
        image.enabled = isEnabled;
        playerSymbol.SetEnable(isEnabled);
        enemies.ForEach(kv => kv.Value?.SetEnable(isEnabled));
    }

    public override void ExpandUI()
    {
        int miniMapSize = Mathf.Min(EXPAND_MAP_SIZE, mapData.width);
        ResetUISize(expandSize, miniMapSize, 1f);
        rectTransform.anchoredPosition = anchoredCenter;

        image.color = Color.white;
        UpdateMiniMap(PlayerInfo.Instance.Vec3Pos, miniMapSize);
    }

    public override void ShrinkUI()
    {
        ResetUISize(currentSize, MINIMAP_SIZE, 0.4f);
        rectTransform.anchoredPosition = currentPos;

        UpdateMiniMap(PlayerInfo.Instance.Vec3Pos);
    }

    protected void ResetUISize(float size, int miniMapSize, float alpha)
    {
        image.texture = renderTexture = new RenderTexture((int)size, (int)size, 0);
        image.color = new Color(1, 1, 1, alpha);
        rectTransform.sizeDelta = new Vector2(size, size);
        uiTileUnit = rectTransform.sizeDelta / (float)miniMapSize;
        playerSymbol.SetSize(uiTileUnit);
        enemies.ForEach(kv => kv.Value.SetSize(uiTileUnit));
        currentMiniMapSize = miniMapSize;
    }

    void OnDestroy()
    {
        renderTexture?.Release();
    }
}
