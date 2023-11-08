using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlaceEnemyGenerator : EnemyGenerator
{
    [SerializeField] private EnemySpawnPoint prefabEnemySpawnPoint = default;

    private EnemyData enemyData;
    private EnemyTypesData enemyTypesData;

    private EnemyType[] enemyTypes;
    private EnemyType RandomEnemyType => enemyTypes[Random.Range(0, enemyTypes.Length)];

    private bool isWitchReserved = false;
    private int witchLevel;

    private WorldMap map;
    private void SetWorldMap(WorldMap map)
    {
        this.map = map;
        enemyTypes = enemyTypesData.Param(map.floor - 1).types;

        enemyTypes.ForEach(type => CreateEnemyPool(type));

        witchLevel = 5 + 2 * (GameInfo.Instance.LastFloor - map.floor);
    }

    private void CreateEnemyPool(EnemyType type)
    {
        if (enemyPool.ContainsKey(type)) return;

        var pool = new GameObject("Enemy Pool: " + type);
        pool.transform.SetParent(transform);
        enemyPool[type] = pool;
    }

    private Dictionary<EnemyType, GameObject> enemyPool = new Dictionary<EnemyType, GameObject>();

    public IEnemyStatus GetAnnaStatus()
    {
        if (enemyPool.ContainsKey(EnemyType.Anna))
        {
            return enemyPool[EnemyType.Anna].transform
                .FirstOrDefault(t => t.gameObject.activeSelf)?
                .GetComponent<IEnemyStatus>();
        }

        return null;
    }

    private List<EnemySpawnPoint> generatorPool = new List<EnemySpawnPoint>();
    private List<DataStoreAgent.EnemyData>[] respawnData;

    protected override void Awake()
    {
        enemyData = ResourceLoader.Instance.enemyData;
        enemyTypesData = ResourceLoader.Instance.enemyTypesData;

        SetWorldMap(GameManager.Instance.worldMap);
        respawnData = new List<DataStoreAgent.EnemyData>[enemyTypesData.Length].Select(_ => new List<DataStoreAgent.EnemyData>()).ToArray();
    }

    /// <summary>
    /// Places enemy generators. Must be called after preparing floor tiles on WorldMap to search space.
    /// </summary>
    public void Place()
    {
        map.roomCenterPos.ForEach(pos => PlaceGenerator(pos));

        int numOfRandomSpawn = (int)(map.width * map.height * 0.012f);

        if (numOfRandomSpawn < 4) return;

        int division = (int)Mathf.Log(numOfRandomSpawn, 4);

        var regions = GetRegions(new Pos(0, 0), new Pos(map.width - 1, map.height - 1), division);

        List<Pos> placeAlready = new List<Pos>();
        for (int i = 0; i < numOfRandomSpawn; i++)
        {
            (Pos leftTop, Pos rightBottom) region = regions[i % regions.Length];

            int x = Random.Range(region.leftTop.x, region.rightBottom.x + 1);
            int y = Random.Range(region.leftTop.y, region.rightBottom.y + 1);

            var spacePos = map.GetGroundPos(new Pos(x, y), placeAlready);

            if (spacePos.IsNull) continue;

            placeAlready.Add(spacePos);

            PlaceGenerator(spacePos);
        }
    }

    private EnemySpawnPoint PlaceGenerator(Pos pos, EnemyType type = EnemyType.None)
        => PlaceGenerator(map.WorldPos(pos), map.GetTile(pos), type);

    private EnemySpawnPoint PlaceGenerator(Vector3 pos, ITile spawnTile, EnemyType type = EnemyType.None)
    {
        var enemyType = (type == EnemyType.None) ? RandomEnemyType : type;

        var generator =
            Instantiate(prefabEnemySpawnPoint, pos, Quaternion.identity)
                .Init(enemyPool[enemyType], spawnTile, enemyData.Param((int)enemyType));

        generator.transform.SetParent(transform);

        generatorPool.Add(generator);

        return generator;
    }

    private (Pos leftTop, Pos rightBottom)[] GetRegions(Pos leftTop, Pos rightBottom, int division = 1, int offset = 0, (Pos, Pos)[] regions = null)
    {
        int numOfRegions = (int)Mathf.Pow(4, division);

        if (regions == null) regions = new (Pos, Pos)[numOfRegions];

        if (division == 0)
        {
            regions[offset] = (leftTop, rightBottom);
            return regions;
        }

        Pos center = (leftTop + rightBottom) / 2;
        int offsetUnit = numOfRegions / 4;

        GetRegions(leftTop, center, division - 1, offset, regions);
        GetRegions(new Pos(center.x + 1, leftTop.y), new Pos(rightBottom.x, center.y), division - 1, offset + offsetUnit, regions);
        GetRegions(new Pos(leftTop.x, center.y + 1), new Pos(center.x, rightBottom.y), division - 1, offset + offsetUnit * 2, regions);
        GetRegions(new Pos(center.x + 1, center.y + 1), rightBottom, division - 1, offset + offsetUnit * 3, regions);

        return regions;
    }

    private List<DataStoreAgent.EnemyData> GetRespawnData(Pos playerPos)
    {
        var store = new List<DataStoreAgent.EnemyData>();
        this.map.ForEachTiles((tile, pos) =>
        {
            if (tile.OnEnemy != null)
            {
                if (tile.AboveEnemy != tile.OnEnemy && tile.AboveEnemy != null)
                {
                    store.Add(new DataStoreAgent.EnemyData(pos, tile.AboveEnemy));
                }

                if (pos != playerPos) store.Add(new DataStoreAgent.EnemyData(pos, tile.OnEnemy));
            }
        });

        enemyPool.ForEach(
            kv => kv.Value.transform?.ForEach(t =>
            {
                if (t.gameObject.activeSelf)
                {
                    var undead = t.GetComponent<EnemyStatus>() as IUndeadStatus;
                    if (undead != null && undead.Life.Value == 0f)
                    {
                        var map = undead.gameObject.GetComponent<MapUtil>();
                        store.Add(new DataStoreAgent.EnemyData(map.onTilePos, undead));
                    }
                }
            })
        );

        return store;
    }

    /// <summary>
    /// [!Caution!] GameManager.worldMap must be updated before respawn enemies. <br />
    /// since EnemyMapUtil refers to GameManager.worldMap on respawn.
    /// </summary>
    public void SwitchWorldMap(WorldMap map)
    {
        // Switch current world map.
        SetWorldMap(map);
        RespawnEnemies();
    }

    private void RespawnEnemies()
    {
        var restore = respawnData[map.floor - 1];

        restore.ForEach(data => Respawn(data));
        // Reserve spawning witch if player has KeyBlade.
        isWitchReserved = ItemInventory.Instance.hasKeyBlade() && restore.Where(data => data.enemyType == EnemyType.Witch).Count() == 0;
        restore.Clear();
    }

    /// <summary>
    /// Spawns witch on moving floor. Must be called after preparing floor tiles on WorldMap to search space.
    /// </summary>
    public void RespawnWitch()
    {
        if (!isWitchReserved) return;

        var info = PlayerInfo.Instance;
        SpawnWitch(map.SearchSpaceNearBy(info.Pos, 3), info.Dir.Backward, 300f);
        isWitchReserved = false;
    }
    public override void DisableEnemyCommandsAll()
    {
        enemyPool.ForEach(
            kv => kv.Value.transform?.ForEach(t => t.GetComponent<InputHandler>().ClearAll())
        );
    }

    public void DestroyAllEnemies()
    {
        enemyPool.ForEach(
            kv => kv.Value.transform?.ForEach(t => t.GetComponent<Reactor>().Destroy())
        );
    }

    public void EraseAllEnemies()
    {
        enemyPool.ForEach(
            kv => kv.Value.transform?.ForEach(t => t.GetComponent<EnemyReactor>().OnOutOfView())
        );
    }

    public void EraseInvisibleEnemies()
    {
        enemyPool.ForEach(
            kv => kv.Value.transform?.ForEach(t => t.GetComponent<EnemyReactor>().EraseIfInvisible())
        );
    }

    public void DestroyAllEnemyGenerators()
    {
        generatorPool.ForEach(generator => Destroy(generator.gameObject));
        generatorPool.Clear();
    }

    public void DisableAllEnemyGenerators()
    {
        generatorPool.ForEach(generator => generator.Disable());
    }

    public void EnableAllEnemyGenerators()
    {
        generatorPool.ForEach(generator => generator.Enable());
    }

    public IEnemyStatus RandomSpawn(Pos pos, IDirection dir, EnemyStatus.ActivateOption option, EnemyStoreData statusData = null)
        => ManualSpawn(RandomEnemyType, pos, dir, option, statusData);

    public IEnemyStatus ManualSpawn(EnemyType type, Pos pos, IDirection dir, EnemyStatus.ActivateOption option, EnemyStoreData statusData = null)
    {
        CreateEnemyPool(type);
        return Spawn(type, pos, dir, option, statusData);
    }

    public IEnemyStatus SpawnWitch(Pos pos, IDirection dir, float waitFrames = 120f)
        => ManualSpawn(EnemyType.Witch, pos, dir, new EnemyStatus.ActivateOption(2f, 0f, false, true, waitFrames), new EnemyStoreData(witchLevel));

    private IEnemyStatus Respawn(DataStoreAgent.EnemyData data)
        => ManualSpawn(data.enemyType, data.pos, data.dir, new EnemyStatus.ActivateOption(data), data.StoreData());

    private IEnemyStatus Spawn(EnemyType type, Pos pos, IDirection dir, EnemyStatus.ActivateOption option, EnemyStoreData statusData)
        => Spawn(enemyPool[type].transform, enemyData.Param((int)type), map.WorldPos(pos), dir, option, statusData);

    public List<DataStoreAgent.EnemyData>[] ExportRespawnData(Pos playerPos)
    {
        respawnData[this.map.floor - 1] = GetRespawnData(playerPos);
        return respawnData;
    }

    public void ImportRespawnData(List<DataStoreAgent.EnemyData>[] import, WorldMap currentMap)
    {
        respawnData = import;
        SwitchWorldMap(currentMap);
    }
}
