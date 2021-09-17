using UnityEngine;

public class Generator<T> : MonoBehaviour
    where T : SpawnObject<T>
{
    [SerializeField] protected T prefab = default;
    protected Transform pool;
    public Vector3 spawnPoint;

    protected virtual void Awake()
    {
        pool = transform;
        spawnPoint = transform.position;
    }

    public virtual T Spawn() => Spawn(Vector3.zero);
    public virtual T Spawn(Vector3 offset)
    {
        foreach (Transform t in pool)
        {
            if (!t.gameObject.activeSelf)
            {
                return t.GetComponent<T>().OnSpawn(spawnPoint + offset);
            }
        }
        return Instantiate(prefab, pool, false).OnSpawn(spawnPoint + offset);
    }
}
