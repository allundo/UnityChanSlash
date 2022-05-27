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

    private WorldMap map;
    private void SetWorldMap(WorldMap map)
    {
        this.map = map;
        enemyTypes = enemyTypesData.Param(map.floor - 1).types;

        enemyTypes.ForEach(type => CreateEnemyPool(type));
    }

    private void CreateEnemyPool(EnemyType type)
    {
        if (enemyPool.ContainsKey(type)) return;

        var pool = new GameObject("Enemy Pool: " + type);
        pool.transform.SetParent(transform);
        enemyPool[type] = pool;
    }

    private Dictionary<EnemyType, GameObject> enemyPool = new Dictionary<EnemyType, GameObject>();

    private List<EnemySpawnPoint> generatorPool = new List<EnemySpawnPoint>();
    private List<RespawnData>[] respawnData;

    protected override void Awake()
    {
        enemyData = ResourceLoader.Instance.enemyData;
        enemyTypesData = ResourceLoader.Instance.enemyTypesData;

        SetWorldMap(GameManager.Instance.worldMap);
        respawnData = new List<RespawnData>[enemyTypesData.Length].Select(_ => new List<RespawnData>()).ToArray();
    }

    /// <summary>
    /// Places enemy generators. Must be called after preparing floor tiles on WorldMap to search space.
    /// </summary>
    public void Place()
    {
        map.roomCenterPos.ForEach(pos => PlaceGenerator(pos));

        int numOfRandomSpawn = (int)(map.Width * map.Height * 0.012f);

        if (numOfRandomSpawn < 4) return;

        int division = (int)Mathf.Log(numOfRandomSpawn, 4);

        var regions = GetRegions(new Pos(0, 0), new Pos(map.Width - 1, map.Height - 1), division);

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

    public void SwitchWorldMap(WorldMap map, Pos playerPos)
    {
        var store = respawnData[this.map.floor - 1];
        var restore = respawnData[map.floor - 1];

        this.map.ForEachTiles((tile, pos) =>
        {
            tile.OnCharacterDest = tile.AboveEnemy = null;
            if (tile.OnEnemy != null)
            {
                if (pos != playerPos) store.Add(new RespawnData(tile.OnEnemy, pos));
                tile.OnEnemy = null;
            }
        });

        DestroyAllEnemies();
        DestroyAllEnemyGenerators();

        SetWorldMap(map);
        restore.ForEach(data => Respawn(data));

        // Reserve spawning witch if player has KeyBlade.
        isWitchReserved = GameManager.Instance.IsPlayerHavingKeyBlade && restore.Where(data => data.type == EnemyType.Witch).Count() == 0;

        restore.Clear();
    }

    /// <summary>
    /// Spawns witch on moving floor. Must be called after preparing floor tiles on WorldMap to search space.
    /// </summary>
    public void RespawnWitch()
    {
        if (!isWitchReserved) return;

        var gm = GameManager.Instance;
        SpawnWitch(map.SearchSpaceNearBy(gm.PlayerPos, 3), gm.PlayerDir.Backward, 300f);
        isWitchReserved = false;
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

    public void DestroyAllEnemyGenerators()
    {
        generatorPool.ForEach(generator => generator.Destroy());
        generatorPool.Clear();
    }

    public IEnemyStatus RandomSpawn(Pos pos, IDirection dir, EnemyStatus.ActivateOption option, float life = 0f)
        => ManualSpawn(RandomEnemyType, pos, dir, option, life);

    public IEnemyStatus ManualSpawn(EnemyType type, Pos pos, IDirection dir, EnemyStatus.ActivateOption option, float life = 0f)
    {
        CreateEnemyPool(type);
        return Spawn(type, pos, dir, option, life);
    }

    public IEnemyStatus SpawnWitch(Pos pos, IDirection dir, float waitFrames = 120f)
        => ManualSpawn(EnemyType.Witch, pos, dir, new EnemyStatus.ActivateOption(2f, true, waitFrames));

    private IEnemyStatus Respawn(RespawnData data) => Spawn(data.type, data.pos, data.dir, new EnemyStatus.ActivateOption(), data.life);

    private IEnemyStatus Spawn(EnemyType type, Pos pos, IDirection dir, EnemyStatus.ActivateOption option, float life)
        => Spawn(enemyPool[type].transform, enemyData.Param((int)type), map.WorldPos(pos), dir, option, life);

    private struct RespawnData
    {
        public RespawnData(IEnemyStatus status, Pos pos)
        {
            this.type = status.type;
            this.pos = pos;
            this.dir = status.dir;
            this.life = status.Life.Value;
        }

        public EnemyType type;
        public Pos pos;
        public IDirection dir;
        public float life;
    }
}
