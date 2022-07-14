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

    public int surplusCoins = 0;

    private readonly Dictionary<BagSize, float> coinScales = new Dictionary<BagSize, float>()
    {
        { BagSize.Small, 0.1f },
        { BagSize.Middle, 0.25f },
        { BagSize.Big, 0.5f },
        { BagSize.Gigantic, 0.5f },
    };

    private readonly Dictionary<BagSize, Vector3> startPositions = new Dictionary<BagSize, Vector3>()
    {
        { BagSize.Small, new Vector3(0, 5f, 0.35f) },
        { BagSize.Middle, new Vector3(0, 5f, 0.4f) },
        { BagSize.Big, new Vector3(0, 5f, 0.65f) },
        { BagSize.Gigantic, new Vector3(0, 5f, -0.25f) },
    };
    private readonly Dictionary<BagSize, Vector3> rightHandOffsets = new Dictionary<BagSize, Vector3>()
    {
        { BagSize.Small, new Vector3(-0.1233f, 0.0695f, -0.0513f) },
        { BagSize.Middle, new Vector3(-0.272f, 0.221f, -0.1f) },
        { BagSize.Big, Vector3.zero },
        { BagSize.Gigantic, Vector3.zero },
    };

    private readonly Dictionary<BagSize, float> dropDelay = new Dictionary<BagSize, float>()
    {
        { BagSize.Small, 0.65f },
        { BagSize.Middle, 1f },
        { BagSize.Big, 1.2f },
        { BagSize.Gigantic, 0.65f },
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

        sphereBody = GetComponent<Rigidbody>();
        sphereBody.useGravity = false;

        bag.clothSolverFrequency = 240;

        for (int i = 0; i < 32; i++)
        {
            coins[i] = Instantiate(prefabCoin, transform);
            coins[i].transform.localScale *= coinScales[bagSize];
            coins[i].gameObject.SetActive(false);
        }
    }

    void Start()
    {
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
    }

    public Tween Drop()
    {
        return DOVirtual.DelayedCall(dropDelay[bagSize], () => sphereBody.useGravity = true);
    }

    public void CaughtBy(Transform parent)
    {
        sphereBody.useGravity = false;
        sphereBody.velocity = Vector3.zero;

        transform.SetParent(parent);
        transform.DOLocalMove(rightHandOffsets[bagSize], 0.5f).Play();
        transform.DOLocalRotate(new Vector3(53.7f, -18.2f, 36.475f), 0.5f).Play();
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
