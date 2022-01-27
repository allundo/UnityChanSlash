using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class EnemyAutoGenerator : EnemyGenerator
{
    protected MobParam param;
    protected ITile spawnTile;
    protected Collider detectCharacter;

    protected Coroutine spawnLoop = null;
    protected Coroutine searchCharacter = null;

    protected override void Awake()
    {
        base.Awake();
        detectCharacter = GetComponent<Collider>();
    }

    public MobStatus Spawn(IDirection dir = null, float life = 0f)
        => base.Spawn(param, spawnPoint, dir, life);

    public EnemyAutoGenerator Init(GameObject enemyPool, ITile tile, MobParam param)
    {
        // Set another parent since Collider cannot detect self child GameObjects.
        pool = enemyPool.transform;
        spawnTile = tile;
        this.param = param;

        return this;
    }

    public IEnumerator SpawnLoop()
    {
        while (true)
        {
            searchCharacter = StartCoroutine(SearchCharacter());
            yield return new WaitForSeconds(10);
        }
    }

    private IEnumerator SearchCharacter()
    {
        detectCharacter.enabled = true;

        yield return new WaitForSeconds(1);

        detectCharacter.enabled = false;
        if (!spawnTile.IsCharacterOn) Spawn();
    }

    public void OnTriggerEnter(Collider other)
    {
        // Cancel spawning when enemy or player is detected nearby
        if (other.GetComponent<MobStatus>() != null) StopSearchCharacter();
    }

    public void Activate()
    {
        gameObject.SetActive(true);
        detectCharacter.enabled = false;
        spawnLoop = StartCoroutine(SpawnLoop());
    }

    public void Inactivate()
    {
        StopCoroutine(spawnLoop);
        StopSearchCharacter();
        gameObject.SetActive(false);
    }

    private void StopSearchCharacter()
    {
        detectCharacter.enabled = false;
        StopCoroutine(searchCharacter);
    }
}