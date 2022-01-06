using UnityEngine;

public class EnemyGenerator : Generator<MobStatus>
{
    /// <summary>
    /// Instantiate on default object pool
    /// </summary>
    public virtual MobStatus Spawn(MobParam param, Vector3 pos, IDirection dir = null, float life = 0f)
        => GetInstance(param.prefab).InitParam(param, life).OnSpawn(pos, dir);

    /// <summary>
    /// Instantiate on the object pool specified by 'pool' parameter
    /// </summary>
    public virtual MobStatus Spawn(Transform pool, MobParam param, Vector3 pos, IDirection dir = null, float life = 0f)
        => GetInstance(pool, param.prefab).InitParam(param, life).OnSpawn(pos, dir);

    public virtual MobStatus GetInstance(Transform pool, MobStatus prefab)
        => GetPooledObj(pool) ?? Instantiate(prefab, pool, false);

    protected virtual MobStatus GetPooledObj(Transform pool)
        => pool.FirstOrDefault(t => !t.gameObject.activeSelf)?.GetComponent<MobStatus>();

    public virtual EnemyGenerator Init(GameObject enemyPool)
    {
        pool = enemyPool.transform;
        return this;
    }

    public override void DestroyAll()
    {
        pool.ForEach(t => t.GetComponent<MobReactor>().Destroy());
    }
}