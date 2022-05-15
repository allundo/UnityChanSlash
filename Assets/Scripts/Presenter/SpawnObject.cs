using UnityEngine;

public abstract class SpawnObject<T> : MonoBehaviour, ISpawnObject<T>
{
    public abstract T OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5f);
    public virtual void Inactivate() { gameObject.SetActive(false); }
    public virtual void Activate() { gameObject.SetActive(true); }
}

public interface ISpawnObject<T>
{
    /// <summary>
    /// Initialization process when spawned by Generator<T>
    /// </summary>
    /// <param name="pos">Object start position on spawned</param>
    /// <param name="dir">Object start direction on spawned</param>
    /// <param name="duration">mainly used as fade in duration time on spawned</param>
    /// <returns></returns>
    T OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5f);
    void Inactivate();
    void Activate();
}
