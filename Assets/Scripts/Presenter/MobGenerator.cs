using UnityEngine;

public class MobGenerator<T> : Generator<T>
    where T : MonoBehaviour, ISpawnObject<T>
{
    [SerializeField] protected T fixedPrefab = default;

    public virtual T Spawn(IDirection dir = null) => Spawn(fixedPrefab, dir);
    public virtual T Spawn(Vector3 offset, IDirection dir = null, float duration = 0.5f)
        => Spawn(fixedPrefab, offset, dir, duration);

    public virtual T Spawn(Vector3 offset, Quaternion rotation, float duration = 0.5f)
        => Spawn(fixedPrefab, offset, rotation, duration);

    protected T GetInstance() => GetInstance(fixedPrefab);
}
