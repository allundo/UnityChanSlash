using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class EnemyGenerator : Generator<MobStatus>
{
    protected ITile spawnTile;
    protected Collider detectCharacter;

    protected Coroutine searchCharacter = null;

    protected virtual void Start()
    {
        detectCharacter = GetComponent<Collider>();

        detectCharacter.enabled = false;
        StartCoroutine(SpawnLoop());
    }

    public void Init(GameObject enemyPool, ITile tile)
    {
        // Set another parent since Collider cannot detect self child GameObjects.
        pool = enemyPool.transform;
        spawnTile = tile;
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
        MobStatus status = other.GetComponent<MobStatus>();

        // Cancel spawning when enemy or player is detected nearby
        if (status != null)
        {
            detectCharacter.enabled = false;
            StopCoroutine(searchCharacter);
        }

    }
}