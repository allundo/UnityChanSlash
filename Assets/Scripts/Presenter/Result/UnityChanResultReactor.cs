using UnityEngine;
using UnityChan;

public interface ISphereColliderPair
{
    ClothSphereColliderPair sphereColliderPair { get; }
}

public class UnityChanResultReactor : MonoBehaviour, ISphereColliderPair
{
    [SerializeField] private SphereCollider headCollider = default;
    [SerializeField] private SphereCollider footCollider = default;

    private CapsuleCollider col;
    private ResultFaceAnimator anim;
    private RandomWind randomWind;

    public ClothSphereColliderPair sphereColliderPair { get; private set; }

    void Awake()
    {
        col = GetComponent<CapsuleCollider>();
        anim = GetComponent<ResultFaceAnimator>();
        randomWind = GetComponent<RandomWind>();

        col.enabled = true;

        sphereColliderPair = new ClothSphereColliderPair(headCollider, footCollider);
    }

    public void SetNormal() => anim.normal.Fire();
    public void SetSmile() => anim.smile.Fire();
    public void SetEyeClose() => anim.eyeClose.Fire();

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BagControl>() == null) return;

        col.enabled = false;

        anim.drop.Fire();

        randomWind.isWindActive = false;
    }
}
