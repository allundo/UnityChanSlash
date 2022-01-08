using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlaceEnemyGenerator : EnemyGenerator
{
    [SerializeField] private EnemyAutoGenerator prefabEnemyGenerator = default;

    [SerializeField] private MobData enemyData = default;
    [SerializeField] private EnemyTypesData enemyTypesData = default;

    [SerializeField] private int numOfRandomSpawn = 12;

    private EnemyType[] enemyTypes;
    private EnemyType RandomEnemyType => enemyTypes[Random.Range(0, enemyTypes.Length)];

    private WorldMap map;
    private void SetWorldMap(WorldMap map)
    {
        this.map = map;
        enemyTypes = enemyTypesData.Param(map.floor - 1).types;

        enemyTypes.ForEach(type =>
        {
            if (!enemyPool.ContainsKey(type)) enemyPool[type] = new GameObject("Enemy Pool: " + type);
        });
    }

    private Dictionary<EnemyType, GameObject> enemyPool = new Dictionary<EnemyType, GameObject>();

    private List<EnemyAutoGenerator> generatorPool = new List<EnemyAutoGenerator>();
    private List<RespawnData>[] respawnData;

    protected override void Awake()
    {
        SetWorldMap(GameManager.Instance.worldMap);
        respawnData = new List<RespawnData>[enemyTypesData.Length].Select(_ => new List<RespawnData>()).ToArray();
    }

    public void Place()
    {
        map.roomCenterPos.ForEach(pos =>
        {
            var enemyType = RandomEnemyType;
            generatorPool.Add(
                Instantiate(prefabEnemyGenerator, map.WorldPos(pos), Quaternion.identity)
                    .Init(enemyPool[enemyType], map.GetTile(pos), enemyData.Param((int)enemyType))
            );
        });

        if (numOfRandomSpawn < 4) return;

        int division = (int)Mathf.Log(numOfRandomSpawn, 4);

        var regions = GetRegions(new Pos(0, 0), new Pos(map.Width - 1, map.Height - 1), division);

        for (int i = 0; i < numOfRandomSpawn; i++)
        {
            (Pos leftTop, Pos rightBottom) region = regions[numOfRandomSpawn % regions.Length];

            int x = Random.Range(region.leftTop.x, region.rightBottom.x + 1);
            int y = Random.Range(region.leftTop.y, region.rightBottom.y + 1);

            Ground ground = map.GetGround(ref x, ref y);

            if (ground == null) continue;

            var enemyType = RandomEnemyType;

            generatorPool.Add(
                Instantiate(prefabEnemyGenerator, map.WorldPos(x, y), Quaternion.identity)
                    .Init(enemyPool[enemyType], ground, enemyData.Param((int)enemyType))
            );
        }
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

    public void SwitchWorldMap(WorldMap map)
    {
        var store = respawnData[this.map.floor - 1];
        var restore = respawnData[map.floor - 1];

        this.map.ForEachTiles((tile, pos) =>
        {
            tile.IsObjectOn = false;
            if (tile.OnEnemy != null)
            {
                store.Add(new RespawnData(tile.OnEnemy, pos));
                tile.OnEnemy = null;
            }
        });

        DestroyAllEnemies();
        DestroyAllEnemyGenerators();

        SetWorldMap(map);
        restore.ForEach(data => Respawn(data));
        restore.Clear();

        Place();
    }

    public void DestroyAllEnemies()
    {
        enemyPool.ForEach(
            kv => kv.Value.transform?.ForEach(t => t.GetComponent<MobReactor>().Destroy())
        );
    }

    public void DestroyAllEnemyGenerators()
    {
        generatorPool.ForEach(generator => Destroy(generator));
        generatorPool.Clear();
    }

    private MobStatus Respawn(RespawnData data)
        => Spawn(enemyPool[data.type].transform, enemyData.Param((int)data.type), map.WorldPos(data.pos), data.dir, data.life);

    private struct RespawnData
    {
        public RespawnData(MobStatus status, Pos pos)
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
