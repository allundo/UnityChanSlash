using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using System;

public class MiniMap : SwitchingUIBase
{
    [SerializeField] private UISymbolGenerator enemyPointGenerator = default;
    [SerializeField] private PlayerSymbol playerSymbol = default;
    [SerializeField] private Button statusBtn = default;

    private static readonly int MINIMAP_SIZE = 15;
    private static readonly int EXPAND_MAP_SIZE = 41;
    private Vector2 uiTileUnit;
    private int currentMiniMapSize = MINIMAP_SIZE;

    private RawImage image;

    private RenderTexture renderTexture;

    private static readonly Vector2 OUT_OF_SCREEN = new Vector2(1024f, 1024f);

    private WorldMap map;

    private Vector3 center;

    private bool isUpdated = false;

    private Dictionary<EnemyReactor, UISymbol> enemies = new Dictionary<EnemyReactor, UISymbol>();

    public IObservable<Unit> Switch => statusBtn.OnClickAsObservable();

    protected override void Awake()
    {
        base.Awake();

        map = GameManager.Instance.worldMap;
        isShown = true;
    }

    void Start()
    {
        Switch.Subscribe(_ => HideMap()).AddTo(this);
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

    public void UpdateMiniMap(Vector3 playerPos) => UpdateMiniMap(playerPos, MINIMAP_SIZE);

    protected void UpdateMiniMap(Vector3 playerPos, int miniMapSize)
    {
        Graphics.Blit(map.GetMiniMap(miniMapSize), renderTexture);

        center = map.MiniMapCenterWorldPos(miniMapSize);
        playerSymbol.SetPos(UIOffsetDiscrete(playerPos));
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

    public void SetEnable(bool isEnabled)
    {
        image.enabled = isEnabled;
        playerSymbol.SetEnable(isEnabled);
        enemies.ForEach(kv => kv.Value?.SetEnable(isEnabled));
    }

    public void ExpandMap()
    {
        int miniMapSize = Mathf.Min(EXPAND_MAP_SIZE, map.Width);
        ResetUISize(expandSize, miniMapSize, 1f);
        rectTransform.anchoredPosition = anchoredCenter;

        image.color = Color.white;
        UpdateMiniMap(PlayerInfo.Instance.PlayerVec3Pos, miniMapSize);

        MoveEnemySymbols();
    }

    public void ShrinkMap()
    {
        ResetUISize(currentSize, MINIMAP_SIZE, 0.4f);
        rectTransform.anchoredPosition = currentPos;

        UpdateMiniMap(PlayerInfo.Instance.PlayerVec3Pos);

        MoveEnemySymbols();
    }

    public void ResetUISize(float size, int miniMapSize, float alpha)
    {
        image.texture = renderTexture = new RenderTexture((int)size, (int)size, 0);
        image.color = new Color(1, 1, 1, alpha);
        rectTransform.sizeDelta = new Vector2(size, size);
        uiTileUnit = rectTransform.sizeDelta / (float)miniMapSize;
        playerSymbol.SetSize(uiTileUnit);
        currentMiniMapSize = miniMapSize;
    }

    private void HideMap(float duration = 0.25f)
    {
        statusBtn.enabled = false;
        HideButton();

        SwitchUI(duration, () => SetEnable(false));
    }

    public void ShowMap(float duration = 0.25f, float delay = 0.25f)
    {
        SetEnable(true);
        ShowButton();

        DOVirtual.DelayedCall(delay, () => SwitchUI(duration, () => statusBtn.enabled = true)).Play();
    }

    public void ShowButton() => statusBtn.gameObject.SetActive(true);
    public void HideButton() => statusBtn.gameObject.SetActive(false);
}