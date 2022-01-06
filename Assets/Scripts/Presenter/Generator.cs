using UnityEngine;

public class Generator<T> : MonoBehaviour
    where T : MonoBehaviour, ISpawnObject<T>
{
    protected Transform pool;
    public Vector3 spawnPoint;

    protected virtual void Awake()
    {
        pool = transform;
        spawnPoint = transform.position;
    }

    public virtual T Spawn(T prefab, IDirection dir = null) => Spawn(prefab, Vector3.zero, dir);
    public virtual T Spawn(T prefab, Vector3 offset, IDirection dir = null, float duration = 0.5f)
    {
        return GetInstance(prefab).OnSpawn(spawnPoint + offset, dir, duration);
    }

    public virtual T Spawn(T prefab, Vector3 offset, Quaternion rotation, float duration = 0.5f)
    {
        T spawnObject = GetInstance(prefab);

        spawnObject.transform.rotation = rotation;

        return spawnObject.OnSpawn(spawnPoint + offset, null, duration);
    }

    /// <summary>
    /// Returns inactivated but instantiated object to respawn.
    /// </summary>
    /// <returns>SpawnObject; null if there is no inactivated(pooled) object</returns>
    protected T GetPooledObj() => pool.FirstOrDefault(t => !t.gameObject.activeSelf)?.GetComponent<T>();

    protected T GetInstance(T prefab) => GetPooledObj() ?? Instantiate(prefab, pool, false);

    public virtual void DestroyAll()
    {
        pool.ForEach(t => Destroy(t.gameObject));
    }
}
