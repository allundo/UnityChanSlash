using UnityEngine;

public class GroundCoinGenerator : MonoBehaviour
{
    [SerializeField] private Rigidbody prefabGroundCoin = default;
    [SerializeField] private GameObject ground = default;

    public GameObject Ground => ground;

    public Rigidbody Spawn(Rigidbody inherit)
    {
        var instance = GetPooledObj() ?? Instantiate(prefabGroundCoin, transform, false);
        instance.transform.position = inherit.transform.position;
        instance.transform.rotation = inherit.transform.rotation;
        instance.velocity = inherit.velocity;
        instance.angularVelocity = inherit.angularVelocity;
        return instance;
    }

    /// <summary>
    /// Returns inactivated but instantiated object to respawn.
    /// </summary>
    protected virtual Rigidbody GetPooledObj() => transform.FirstOrDefault(t => !t.gameObject.activeSelf)?.GetComponent<Rigidbody>();

    public virtual void DestroyAll()
    {
        transform.ForEach(t => Destroy(t.gameObject));
    }
}
