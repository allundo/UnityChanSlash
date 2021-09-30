using UnityEngine;

public class Generator<T> : MonoBehaviour
    where T : MonoBehaviour, ISpawnObject<T>
{
    [SerializeField] protected T prefab = default;
    protected Transform pool;
    public Vector3 spawnPoint;

    protected virtual void Awake()
    {
        pool = transform;
        spawnPoint = transform.position;
    }

    public virtual T Spawn(IDirection dir = null) => Spawn(Vector3.zero, dir);
    public virtual T Spawn(Vector3 offset, IDirection dir = null, float duration = 0.5f)
    {
        return GetInstance().OnSpawn(spawnPoint + offset, dir, duration);
    }

    public virtual T Spawn(Vector3 offset, Quaternion rotation, float duration = 0.5f)
    {
        T spawnObject = GetInstance();

        spawnObject.transform.rotation = rotation;

        return spawnObject.OnSpawn(spawnPoint + offset, null, duration);
    }

    /// <summary>
    /// Returns inactivated but instantiated object to respawn.
    /// </summary>
    /// <returns>SpawnObject; null if there is no inactivated(pooled) object</returns>
    protected T GetPooledObj() => pool.FirstOrDefault(t => !t.gameObject.activeSelf)?.GetComponent<T>();

    protected T GetInstance() => GetPooledObj() ?? Instantiate(prefab, pool, false);
}
