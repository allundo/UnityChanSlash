using System.Collections;
using UnityEngine;

public class GroundCoinGenerator : MonoBehaviour
{
    [SerializeField] private Rigidbody prefabGroundCoin = default;
    [SerializeField] private GameObject ground = default;

    public GameObject Ground => ground;

    public Rigidbody Spawn(Rigidbody inherit)
    {
        var instance = GetPooledObj() ?? GetNewInstantiate();
        instance.transform.position = inherit.transform.position;
        instance.transform.rotation = inherit.transform.rotation;
        instance.velocity = inherit.velocity;
        instance.angularVelocity = inherit.angularVelocity;
        instance.gameObject.SetActive(true);
        return instance;
    }

    public Rigidbody GetNewInstantiate() => Instantiate(prefabGroundCoin, transform, false);
    public void PoolCoins(int surplusCoins) => StartCoroutine(PoolCoinsCoroutine(surplusCoins));

    private IEnumerator PoolCoinsCoroutine(int surplusCoins)
    {
        int poolCoins = Mathf.Min(surplusCoins, 48); // Accept max 48 coins for pooling.

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < poolCoins; i++)
        {
            GetNewInstantiate().gameObject.SetActive(false);
            yield return new WaitForSeconds(0.1f);
        }
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
