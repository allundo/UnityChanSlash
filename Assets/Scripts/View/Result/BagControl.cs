using System.Linq;
using System.Collections;
using UnityEngine;

public class BagControl : MonoBehaviour
{
    [SerializeField] public CapsuleCollider target = default;
    [SerializeField] public CapsuleCollider prefabCoin = default;
    [SerializeField] public Cloth bag = default;
    [SerializeField] public ReverseMeshBox box = default;

    private CapsuleCollider[] coins = new CapsuleCollider[15];
    private Rigidbody sphereBody;

    void Start()
    {
        sphereBody = GetComponent<Rigidbody>();
        sphereBody.useGravity = false;

        for (int i = 1; i < 15; i++)
        {
            coins[i] = Instantiate(prefabCoin);
            coins[i].gameObject.SetActive(false);
        }

        coins[0] = target;

        bag.capsuleColliders = coins.Select(ball => ball.GetComponent<CapsuleCollider>()).ToArray();
        StartCoroutine(ActiveSequence());
    }

    private IEnumerator ActiveSequence()
    {
        for (int i = 1; i < 15; i++)
        {
            box.InsertCoin(coins[i].gameObject);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(9f);

        sphereBody.useGravity = true;
    }
}
