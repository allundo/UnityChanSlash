using UnityEngine;
using System.Linq;

public class SpawnHandler : SingletonMonoBehaviour<SpawnHandler>
{
    [SerializeField] private PlaceEnemyGenerator placeEnemyGenerator = default;
    [SerializeField] private ItemGenerator itemGenerator = default;
    [SerializeField] private BulletGeneratorLoader bulletGeneratorLoader = default;
    [SerializeField] private WitchLightGenerator lightGenerator = default;
    [SerializeField] private LifeGaugeGenerator lifeGaugeGenerator = default;
    [SerializeField] private DebugEnemyGenerator[] debugEnemyGenerators = default;

    public BulletGenerator GetBulletGenerator(BulletType type) => bulletGeneratorLoader.bulletGenerators[type];
    public LifeGaugeGenerator GetLifeGaugeGenerator() => lifeGaugeGenerator;

    public void PlaceEnemyGenerators() => placeEnemyGenerator.Place();

    public void ActivateDebugEnemyGenerators()
    {
        debugEnemyGenerators.ForEach(gen => gen.gameObject.SetActive(true));
    }

    public void SpawnLight(Vector3 pos) => lightGenerator.Spawn(pos);
    public void DistributeLight(Vector3 pos, float range) => lightGenerator.Spawn(pos + UnityEngine.Random.insideUnitSphere * range);

    public IEnemyStatus PlaceEnemy(EnemyType type, Pos pos, IDirection dir, EnemyStatus.ActivateOption option, EnemyStoreData data = null)
        => placeEnemyGenerator.ManualSpawn(type, pos, dir, option, data);

    public IEnemyStatus PlaceEnemyRandom(Pos pos, IDirection dir, EnemyStatus.ActivateOption option, EnemyStoreData data = null)
        => placeEnemyGenerator.RandomSpawn(pos, dir, option, data);

    public IEnemyStatus PlaceWitch(Pos pos, IDirection dir, float waitFrames = 120f)
        => placeEnemyGenerator.SpawnWitch(pos, dir, waitFrames);

    public void RespawnWitch() => placeEnemyGenerator.RespawnWitch();

    public void DisableAllEnemiesInput()
    {
        debugEnemyGenerators.ForEach(gen => gen.DisableInputAll());
        placeEnemyGenerator.DisableAllEnemiesInput();
    }

    public void EraseAllEnemies() => placeEnemyGenerator.EraseAllEnemies();

    public void MoveFloorCharacters(WorldMap map, Pos startPos)
    {
        EnemyCommand.ClearResetTweens();

        // Do not destroyLifeGauges since they are reusable for all over floors.
        // lifeGaugeGenerator.DestroyAll();

        // Enemies and bullets must be destroyed during the same frame.
        placeEnemyGenerator.SwitchWorldMap(map, startPos);

        debugEnemyGenerators.ForEach(gen =>
        {
            gen.DestroyAll();
            gen.Inactivate();
        });

        lifeGaugeGenerator.DestroyAll();

        // Some bullets refer to a character status, so don't call Update() after the enemies had been destroyed.
        bulletGeneratorLoader.DestroyAll();

        lightGenerator.DestroyAll();
    }

    public Item PlaceItem(ItemType type, int numOfItem, Pos pos) => itemGenerator.PlaceItem(type, numOfItem, pos, PlayerInfo.Instance.Dir);

    public void MoveFloorItems(WorldMap map, IDirection playerDir)
    {
        itemGenerator.SwitchWorldMap(map);
    }

    public DataStoreAgent.RespawnData[] ExportRespawnData()
    {
        int lastFloor = GameInfo.Instance.LastFloor;

        var export = new DataStoreAgent.RespawnData[lastFloor];
        var enemyData = placeEnemyGenerator.ExportRespawnData();
        var itemData = itemGenerator.ExportRespawnData();

        for (int i = 0; i < lastFloor; i++)
        {
            export[i] = new DataStoreAgent.RespawnData(enemyData[i], itemData[i]);
        }
        return export;
    }

    public void ImportRespawnData(DataStoreAgent.RespawnData[] import, WorldMap map)
    {
        placeEnemyGenerator.ImportRespawnData(import.Select(data => data.enemyData.ToList()).ToArray(), map);
        itemGenerator.ImportRespawnData(import.Select(data => data.itemData.ToList()).ToArray(), map);
    }
}