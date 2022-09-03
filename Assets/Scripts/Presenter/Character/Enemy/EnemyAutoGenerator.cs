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

    protected virtual void Start()
    {
        gameObject.SetActive(false);
    }

    public IStatus Spawn(IDirection dir = null, Status.StoreData data = null)
        => base.Spawn(param, spawnPoint, dir, data);

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
        if (gameObject.activeSelf) return;

        gameObject.SetActive(true);
        detectCharacter.enabled = false;
        spawnLoop = StartCoroutine(SpawnLoop());
    }

    public void Inactivate()
    {
        if (!gameObject.activeSelf) return;

        StopCoroutine(spawnLoop);
        StopSearchCharacter();
        gameObject.SetActive(false);
    }

    private void StopSearchCharacter()
    {
        detectCharacter.enabled = false;
        if (searchCharacter != null) StopCoroutine(searchCharacter);
    }
}
