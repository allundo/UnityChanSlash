using UnityEngine;
using UniRx;
using DG.Tweening;

public class BagControl
{
    private YenBag bag;
    private YenBagSource bagSource;
    public Transform bagTf { get; private set; }
    public BagSize bagSize { get; private set; }

    private Tween bagMoveTween;
    private Tween bagRotateTween;

    public int surplusCoins { get; private set; } = 0;

    private Rigidbody sphereBody;

    public BagControl(ulong wagesAmount, GroundCoinGenerator generator)
    {
        if (wagesAmount > 10000000)
        {
            bagSize = BagSize.Gigantic;
            surplusCoins = Mathf.Min((int)((wagesAmount - 10000000) / 500), 640); // Accept max 640 coins for shower.
            generator.PoolCoins(surplusCoins);
        }
        else if (wagesAmount > 2000000)
        {
            bagSize = BagSize.Big;
        }
        else if (wagesAmount > 500000)
        {
            bagSize = BagSize.Middle;
        }
        else
        {
            bagSize = BagSize.Small;
        }

        bagSource = ResourceLoader.Instance.YenBagSource(bagSize);
        bag = Util.Instantiate(bagSource.prefabYenBag);

        sphereBody = bag.GetComponent<Rigidbody>();
        sphereBody.useGravity = false;

        bagTf = bag.transform;
        bagTf.position = bagSource.startPosition;

        bag.GenerateCoins(bagSource.coinScale, generator);
        bag.Activate();

        bag.Caught.Subscribe(catcher => CaughtBy(catcher)).AddTo(bag);
    }

    public void SetPressTarget(ClothSphereColliderPair target)
    {
        bag.cloth.sphereColliders = new ClothSphereColliderPair[] { target };
    }

    public Tween Drop()
    {
        if (bagSize == BagSize.Gigantic) bag.CoinShower(surplusCoins);
        return DOVirtual.DelayedCall(bagSource.dropDelay, () => sphereBody.useGravity = true);
    }

    public void StopCoinShower() => bag.StopCoinShower();

    private void CaughtBy(Transform parent)
    {
        sphereBody.useGravity = false;
        sphereBody.velocity = Vector3.zero;

        bagTf.SetParent(parent);
        bagMoveTween = bagTf.DOLocalMove(bagSource.rightHandOffsets[0], 0.5f).Play();
        bagRotateTween = bagTf.DOLocalRotate(bagSource.catchAngles[0], 0.5f).Play();
    }

    public void UpHoldResetPosition()
    {
        bagMoveTween?.Kill();
        bagRotateTween?.Kill();
        bagTf.DOLocalMove(bagSource.rightHandOffsets[1], 0.5f).Play();
        bagTf.DOLocalRotate(bagSource.catchAngles[1], 0.5f).Play();
    }

    public void CarryingResetPosition()
    {
        bagTf.DOLocalMove(bagSource.rightHandOffsets[2], 0.5f).Play();
        bagTf.DOLocalRotate(bagSource.catchAngles[2], 0.5f).Play();
    }


    public void Destroy() => bag.Destroy();
}
