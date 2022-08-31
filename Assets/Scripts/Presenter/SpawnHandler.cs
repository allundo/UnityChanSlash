using UnityEngine;
using System.Collections;

public class SpawnHandler : SingletonMonoBehaviour<SpawnHandler>
{
    [SerializeField] private PlaceEnemyGenerator placeEnemyGenerator = default;
    [SerializeField] private ItemGenerator itemGenerator = default;
    [SerializeField] private BulletGeneratorLoader bulletGeneratorLoader = default;
    [SerializeField] private WitchLightGenerator lightGenerator = default;
    [SerializeField] private DebugEnemyGenerator[] debugEnemyGenerators = default;

    public BulletGenerator GetBulletGenerator(BulletType type) => bulletGeneratorLoader.bulletGenerators[type];

    public void PlaceEnemyGenerators() => placeEnemyGenerator.Place();

    public void ActivateDebugEnemyGenerators()
    {
        debugEnemyGenerators.ForEach(gen => gen.gameObject.SetActive(true));
    }

    public void SpawnLight(Vector3 pos) => lightGenerator.Spawn(pos);
    public void DistributeLight(Vector3 pos, float range) => lightGenerator.Spawn(pos + UnityEngine.Random.insideUnitSphere * range);

    public IEnemyStatus PlaceEnemy(EnemyType type, Pos pos, IDirection dir, EnemyStatus.ActivateOption option, float life = 0f)
        => placeEnemyGenerator.ManualSpawn(type, pos, dir, option, life);

    public IEnemyStatus PlaceEnemyRandom(Pos pos, IDirection dir, EnemyStatus.ActivateOption option, float life = 0f)
        => placeEnemyGenerator.RandomSpawn(pos, dir, option, life);

    public IEnemyStatus PlaceWitch(Pos pos, IDirection dir, float waitFrames = 120f)
        => placeEnemyGenerator.SpawnWitch(pos, dir, waitFrames);

    public void RespawnWitch() => placeEnemyGenerator.RespawnWitch();

    public void DisableAllEnemiesInput()
    {
        debugEnemyGenerators.ForEach(gen => gen.DisableInputAll());
        placeEnemyGenerator.DisableAllEnemiesInput();
    }

    public void EraseAllEnemies() => placeEnemyGenerator.EraseAllEnemies();

    public void MoveFloorCharacters(WorldMap map, Pos playerPos)
    {
        // Enemies and bullets must be destroyed during the same frame.
        placeEnemyGenerator.SwitchWorldMap(map, playerPos);

        debugEnemyGenerators.ForEach(gen =>
        {
            gen.DestroyAll();
            gen.gameObject.SetActive(false);
        });

        // Some bullets refer to a character status, so don't call Update() after the enemies had been destroyed.
        bulletGeneratorLoader.DestroyAll();

        lightGenerator.DestroyAll();
    }

    public void MoveFloorItems(WorldMap map, IDirection playerDir)
    {
        itemGenerator.SwitchWorldMap(map);
        itemGenerator.Turn(playerDir);
    }
}