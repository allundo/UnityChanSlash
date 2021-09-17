using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class EnemyGenerator : MonoBehaviour
{
    [SerializeField] protected MobReactor enemyPrefab = default;
    protected Transform pool;
    protected Pos spawnPoint;
    protected Tile spawnTile;
    protected Collider detectCharacter;

    protected Coroutine searchCharacter = null;

    protected virtual void Start()
    {
        detectCharacter = GetComponent<Collider>();

        detectCharacter.enabled = false;
        StartCoroutine(SpawnLoop());
    }

    public void Init(GameObject enemyPool, Tile tile, Pos pos)
    {
        pool = enemyPool.transform;
        spawnTile = tile;
        spawnPoint = pos;
    }

    protected void Spawn()
    {
        foreach (Transform t in pool)
        {
            if (!t.gameObject.activeSelf)
            {
                t.GetComponent<MobReactor>().OnSpawn(spawnPoint);
                return;
            }
        }
        Instantiate(enemyPrefab, pool, false).OnSpawn(spawnPoint);
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
        if (!spawnTile.IsCharactorOn) Spawn();
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