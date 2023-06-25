using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class EnemyAutoGenerator : EnemyGenerator
{
    protected EnemyParam param;
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

    public IStatus Spawn(IDirection dir = null, EnemyStoreData data = null)
        => base.Spawn(param, spawnPoint, dir, data);

    public EnemyAutoGenerator Init(GameObject enemyPool, ITile tile, EnemyParam param)
    {
        // Set another parent since Collider cannot detect self child GameObjects.
        pool = enemyPool.transform;
        spawnTile = tile;
        this.param = param;

        return this;
    }

    public IEnumerator SpawnLoop()
    {
        var waitFor10Sec = new WaitForSeconds(10);
        while (true)
        {
            searchCharacter = StartCoroutine(SearchCharacter());
            yield return waitFor10Sec;
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
        if (
            // Cancel spawning when enemy or player is detected nearby.
            other.GetComponent<MobStatus>() != null

            // Cancel spawning if this point is inside player view.
            || (other.GetComponent<MiniMapHandler>() != null && GameManager.Instance.worldMap.miniMapData.IsCurrentViewOpen(spawnPoint))
        )
        {
            StopSearchCharacter();
        }
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
