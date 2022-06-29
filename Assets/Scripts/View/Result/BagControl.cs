using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BagControl : MonoBehaviour
{
    [SerializeField] public CapsuleCollider prefabCoin = default;
    [SerializeField] public Cloth bag = default;
    [SerializeField] public ReverseMeshBox box = default;
    [SerializeField] public BagSize bagSize = default;

    private readonly Dictionary<BagSize, float> coinScales = new Dictionary<BagSize, float>()
    {
        { BagSize.Small, 0.1f },
        { BagSize.Middle, 0.25f },
        { BagSize.Big, 0.5f },
        { BagSize.Gigantic, 1f },
    };

    private readonly Dictionary<BagSize, Vector3> startPositions = new Dictionary<BagSize, Vector3>()
    {
        { BagSize.Small, new Vector3(0, 5f, 0.1f) },
        { BagSize.Middle, new Vector3(0, 5f, 0.15f) },
        { BagSize.Big, new Vector3(0, 5f, 0.2f) },
        { BagSize.Gigantic, new Vector3(0, 5f, -0.25f) },
    };
    private readonly Dictionary<BagSize, Vector3> rightHandOffsets = new Dictionary<BagSize, Vector3>()
    {
        { BagSize.Small, new Vector3(0, 5f, 0.1f) },
        { BagSize.Middle, new Vector3(0, 5f, 0.15f) },
        { BagSize.Big, new Vector3(0, 5f, 0.2f) },
        { BagSize.Gigantic, Vector3.zero },
    };

    private CapsuleCollider[] coins = new CapsuleCollider[32];
    private Rigidbody sphereBody;

    public void SetPressTarget(ClothSphereColliderPair target)
    {
        bag.sphereColliders = new ClothSphereColliderPair[] { target };
    }

    void Awake()
    {
        transform.position = startPositions[bagSize];

        for (int i = 0; i < 32; i++)
        {
            coins[i] = Instantiate(prefabCoin);
            coins[i].transform.localScale *= coinScales[bagSize];
            coins[i].gameObject.SetActive(false);
        }
    }

    void Start()
    {
        sphereBody = GetComponent<Rigidbody>();
        sphereBody.useGravity = false;

        bag.capsuleColliders = coins.Select(ball => ball.GetComponent<CapsuleCollider>()).ToArray();
        StartCoroutine(ActiveSequence());

        // BUG: Instantiated object without gravity still has small down velocity.
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    private IEnumerator ActiveSequence()
    {
        for (int i = 0; i < 14; i++)
        {
            box.InsertCoin(coins[i].gameObject);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void Drop()
    {
        sphereBody.useGravity = true;
    }

    public void CaughtBy(Transform parent)
    {
        sphereBody.useGravity = false;
        transform.SetParent(parent);
        transform.DOLocalMove(rightHandOffsets[bagSize], 0.5f).Play();
    }

    public void Destroy()
    {
        for (int i = 0; i < 32; i++)
        {
            Object.Destroy(coins[i].gameObject);
        }

        Destroy(gameObject);
    }
}
