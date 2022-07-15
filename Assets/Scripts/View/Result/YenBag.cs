using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UniRx;

public class YenBag : MonoBehaviour
{
    [SerializeField] public CapsuleCollider prefabCoin = default;
    [SerializeField] public Cloth cloth = default;
    [SerializeField] public ReverseMeshBox box = default;

    private CapsuleCollider[] coins = new CapsuleCollider[32];
    private ISubject<Transform> caughtSubject = new Subject<Transform>();
    public IObservable<Transform> Caught => caughtSubject;

    public void CaughtBy(Transform catcherTf)
    {
        caughtSubject.OnNext(catcherTf);
        caughtSubject.OnCompleted();
    }

    public void GenerateCoins(float coinScale)
    {
        cloth.clothSolverFrequency = 240;

        float localCoinScale = coinScale / transform.localScale.x;

        for (int i = 0; i < 32; i++)
        {
            coins[i] = Instantiate(prefabCoin, transform);
            coins[i].transform.localScale *= localCoinScale;
            coins[i].gameObject.SetActive(false);
        }
    }

    public void Activate()
    {
        StartCoroutine(ActiveSequence());
    }

    private IEnumerator ActiveSequence()
    {
        cloth.capsuleColliders = coins.Select(coin => coin.GetComponent<CapsuleCollider>()).ToArray();
        yield return null;

        for (int i = 0; i < 14; i++)
        {
            box.InsertCoin(coins[i].gameObject);
            yield return new WaitForSeconds(0.1f);
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
}
