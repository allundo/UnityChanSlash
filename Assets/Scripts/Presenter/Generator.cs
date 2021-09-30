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
    public virtual T Spawn(Vector3 offset, IDirection dir = null)
    {
        foreach (Transform t in pool)
        {
            if (!t.gameObject.activeSelf)
            {
                return t.GetComponent<T>().OnSpawn(spawnPoint + offset, dir);
            }
        }
        return Instantiate(prefab, pool, false).OnSpawn(spawnPoint + offset, dir);
    }
}
