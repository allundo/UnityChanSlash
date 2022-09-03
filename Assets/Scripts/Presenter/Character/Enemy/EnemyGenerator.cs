using UnityEngine;

public class EnemyGenerator : Generator<Status>
{
    protected override void Awake()
    {
        pool = transform;
        spawnPoint = transform.position;
    }

    /// <summary>
    /// Instantiate on default object pool
    /// </summary>
    public virtual IStatus Spawn(MobParam param, Vector3 pos, IDirection dir = null, Status.StoreData data = null)
        => GetInstance(param.prefab).InitParam(param, data).OnSpawn(pos, dir);

    /// <summary>
    /// Instantiate on the object pool specified by 'pool' parameter
    /// </summary>
    public virtual IEnemyStatus Spawn(Transform pool, MobParam param, Vector3 pos, IDirection dir = null, Status.StoreData data = null)
        => Spawn(pool, param, pos, dir, new EnemyStatus.ActivateOption(), data);

    public virtual IEnemyStatus Spawn(Transform pool, MobParam param, Vector3 pos, IDirection dir, EnemyStatus.ActivateOption option, Status.StoreData data = null)
        => (GetInstance(pool, param.prefab).InitParam(param, data) as IEnemyStatus).OnSpawn(pos, dir, option);

    public virtual IStatus GetInstance(Transform pool, Status prefab)
        => GetPooledObj(pool) ?? Instantiate(prefab, pool, false);

    protected virtual IStatus GetPooledObj(Transform pool)
        => pool.FirstOrDefault(t => !t.gameObject.activeSelf)?.GetComponent<Status>();

    public virtual EnemyGenerator Init(GameObject enemyPool)
    {
        pool = enemyPool.transform;
        return this;
    }

    public void DisableInputAll()
    {
        pool.ForEach(t => t.GetComponent<InputHandler>().DisableInput());
    }

    public override void DestroyAll()
    {
        pool.ForEach(t => t.GetComponent<Reactor>().Destroy());
    }
}
