using System;
using System.Collections;
using UnityEngine;
using UniRx;

public class YenBag : MonoBehaviour
{
    [SerializeField] public CapsuleCollider prefabCoin = default;
    [SerializeField] public CapsuleCollider prefabOuterCoin = default;
    [SerializeField] public Cloth cloth = default;
    [SerializeField] public ReverseMeshBox box = default;

    private readonly int NUM_OF_INSERT_COINS = 14;

    private CapsuleCollider[] coins = new CapsuleCollider[32];
    private CapsuleCoin[] outerCoins = new CapsuleCoin[32];

    private ISubject<Transform> caughtSubject = new Subject<Transform>();
    public IObservable<Transform> Caught => caughtSubject;

    private GroundCoinGenerator generator;
    private Coroutine coinShowerCoroutine = null;

    public void StopCoinShower()
    {
        if (coinShowerCoroutine != null)
        {
            StopCoroutine(coinShowerCoroutine);
            outerCoins.ForEach(coin => coin.gameObject.SetActive(false));
        }
    }

    public void CaughtBy(Transform catcherTf)
    {
        caughtSubject.OnNext(catcherTf);
        caughtSubject.OnCompleted();
        StartCoroutine(CoinsRestrictCoroutine());
    }

    /// <summary>
    /// Insert coins again if coins are out of reverse mesh box.
    /// </summary>
    private IEnumerator CoinsRestrictCoroutine()
    {
        var scale = transform.localScale.x * box.transform.localScale.x;
        var sqrMaxDistance = scale * scale * 0.5f;
        yield return new WaitForSeconds(0.1f);
        yield return RespawnCoins(sqrMaxDistance);
        yield return RespawnCoins(sqrMaxDistance);
        yield return RespawnCoins(sqrMaxDistance);
    }

    private IEnumerator RespawnCoins(float sqrMaxDistance)
    {
        for (int i = 0; i < NUM_OF_INSERT_COINS; i++)
        {
            var boxPos = box.transform.position;
            if ((coins[i].transform.position - boxPos).sqrMagnitude > sqrMaxDistance)
            {
                coins[i].transform.position = boxPos;
            }
            yield return null;
        }
    }

    public void GenerateCoins(float coinScale, GroundCoinGenerator generator)
    {
        cloth.clothSolverFrequency = 240;

        float localCoinScale = coinScale / transform.localScale.x;

        for (int i = 0; i < NUM_OF_INSERT_COINS; i++)
        {
            coins[i] = Instantiate(prefabCoin, transform);
            coins[i].transform.localScale *= localCoinScale;
            coins[i].gameObject.SetActive(false);
        }

        for (int i = NUM_OF_INSERT_COINS; i < 32; i++)
        {
            coins[i] = Instantiate(prefabOuterCoin, transform);
            coins[i].gameObject.SetActive(false);

            outerCoins[i] = coins[i].GetComponent<CapsuleCoin>();
            outerCoins[i].SetGenerator(generator);
        }

        this.generator = generator;
    }

    public void Activate()
    {
        StartCoroutine(ActiveSequence());
    }

    private IEnumerator ActiveSequence()
    {
        cloth.capsuleColliders = coins;
        yield return null;

        var waitFor100mSec = new WaitForSeconds(0.1f);
        for (int i = 0; i < NUM_OF_INSERT_COINS; i++)
        {
            box.InsertCoin(coins[i].gameObject);
            yield return waitFor100mSec;
        }
    }

    public void Destroy()
    {
        for (int i = 0; i < 32; i++)
        {
            Destroy(coins[i].gameObject);
        }

        Destroy(gameObject);
    }

    public void CoinShower(int surplusCoins)
    {
        coinShowerCoroutine = StartCoroutine(GenerateCoinsCoroutine(surplusCoins));
    }

    private bool DropCoin()
    {
        for (int i = 0; i < 32; i++)
        {
            if (!outerCoins[i].gameObject.activeSelf)
            {
                outerCoins[i].Activate();
                return true;
            }
        }

        return false;
    }

    private IEnumerator GenerateCoinsCoroutine(int surplusCoins)
    {
        yield return new WaitForSeconds(2f);

        ExchangeInnerCoins();
        cloth.damping = 0.999f;

        // Inactivate box collider not to catch outer coins
        box.gameObject.SetActive(false);

        while (surplusCoins > 0)
        {
            if (DropCoin()) surplusCoins--;

            yield return null;
        }
    }

    private void ExchangeInnerCoins()
    {
        for (int i = 0; i < NUM_OF_INSERT_COINS; i++)
        {
            Destroy(coins[i].gameObject);

            coins[i] = Instantiate(prefabOuterCoin, transform);
            coins[i].gameObject.SetActive(false);

            outerCoins[i] = coins[i].GetComponent<CapsuleCoin>();
            outerCoins[i].SetGenerator(generator);
        }

        cloth.capsuleColliders = coins;
    }
}
