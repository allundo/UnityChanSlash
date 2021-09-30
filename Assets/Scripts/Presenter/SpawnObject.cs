using UnityEngine;

public abstract class SpawnObject<T> : MonoBehaviour, ISpawnObject<T>
{
    public abstract T OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5f);
    public virtual void Inactivate() { gameObject.SetActive(false); }
    public virtual void Activate() { gameObject.SetActive(true); }
}

public interface ISpawnObject<T>
{
    T OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5f);
    void Inactivate();
    void Activate();
}
