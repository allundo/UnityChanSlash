using UnityEngine;
using UniRx;
using DG.Tweening;

public class BagControl
{
    private YenBag bag;
    private YenBagSource bagSource;
    public Transform bagTf { get; private set; }
    public BagSize bagSize { get; private set; }

    public int surplusCoins { get; private set; } = 0;

    private Rigidbody sphereBody;

    public BagControl(ulong wagesAmount)
    {
        if (wagesAmount > 10000000)
        {
            bagSize = BagSize.Gigantic;
            surplusCoins = (int)((wagesAmount - 10000000) / 500);
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

        bag.GenerateCoins(bagSource.coinScale);
        bag.Activate();

        bag.Caught.Subscribe(catcher => CaughtBy(catcher)).AddTo(bag);
    }

    public void SetPressTarget(ClothSphereColliderPair target)
    {
        bag.cloth.sphereColliders = new ClothSphereColliderPair[] { target };
    }

    public Tween Drop()
    {
        return DOVirtual.DelayedCall(bagSource.dropDelay, () => sphereBody.useGravity = true);
    }

    private void CaughtBy(Transform parent)
    {
        sphereBody.useGravity = false;
        sphereBody.velocity = Vector3.zero;

        bagTf.SetParent(parent);
        bagTf.DOLocalMove(bagSource.rightHandOffset, 0.5f).Play();
        bagTf.DOLocalRotate(new Vector3(53.7f, -18.2f, 36.475f), 0.5f).Play();
    }

    public void Destroy() => bag.Destroy();
}
