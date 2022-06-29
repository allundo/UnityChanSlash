using System.Linq;
using System.Collections;
using UnityEngine;

public class BagControl : MonoBehaviour
{
    [SerializeField] public UnityChanResultReactor unityChanReactor = default;
    [SerializeField] public CapsuleCollider prefabCoin = default;
    [SerializeField] public Cloth bag = default;
    [SerializeField] public ReverseMeshBox box = default;
    [SerializeField] public BagSize bagSize = default;

    private CapsuleCollider[] coins = new CapsuleCollider[14];
    private Rigidbody sphereBody;

    public ISphereColliderPair target { private get; set; }

    void Awake()
    {
        target = unityChanReactor;
    }

    void Start()
    {
        sphereBody = GetComponent<Rigidbody>();
        sphereBody.useGravity = false;

        for (int i = 0; i < 14; i++)
        {
            coins[i] = Instantiate(prefabCoin);
            coins[i].gameObject.SetActive(false);
        }

        bag.sphereColliders = new ClothSphereColliderPair[] { target.sphereColliderPair };
        bag.capsuleColliders = coins.Select(ball => ball.GetComponent<CapsuleCollider>()).ToArray();
        StartCoroutine(ActiveSequence());
    }

    private IEnumerator ActiveSequence()
    {
        for (int i = 0; i < 14; i++)
        {
            box.InsertCoin(coins[i].gameObject);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(9f);

        sphereBody.useGravity = true;
    }

    public void Destroy()
    {
        for (int i = 0; i < 14; i++)
        {
            Object.Destroy(coins[i].gameObject);
        }

        Destroy(gameObject);
    }
}
