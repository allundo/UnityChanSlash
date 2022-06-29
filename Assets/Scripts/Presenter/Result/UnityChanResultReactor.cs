using UnityEngine;
using UnityChan;
using UniRx;
using System;

public class UnityChanResultReactor : MonoBehaviour
{
    [SerializeField] private SphereCollider headCollider = default;
    [SerializeField] private SphereCollider footCollider = default;
    [SerializeField] private SphereCollider handCollider = default;

    private CapsuleCollider col;
    private ResultFaceAnimator anim;
    private RandomWind randomWind;

    public ClothSphereColliderPair sphereColliderPair { get; private set; }

    public IObservable<Unit> ScreenOut => screenOutSubject;
    private ISubject<Unit> screenOutSubject = new Subject<Unit>();

    void Awake()
    {
        col = GetComponent<CapsuleCollider>();
        anim = GetComponent<ResultFaceAnimator>();
        randomWind = GetComponent<RandomWind>();

        headCollider.enabled = footCollider.enabled = false;
        sphereColliderPair = new ClothSphereColliderPair(headCollider, footCollider);
    }

    public void SetNormal() => anim.normal.Fire();
    public void SetSmile() => anim.smile.Fire();
    public void SetEyeClose() => anim.eyeClose.Fire();

    public void WalkStart() => screenOutSubject.OnNext(Unit.Default);
    public void WalkEnd() => screenOutSubject.OnCompleted();

    public void FallGround() =>
        Observable
            .Timer(TimeSpan.FromSeconds(3f))
            .Subscribe(
                _ => screenOutSubject.OnNext(Unit.Default),
                screenOutSubject.OnCompleted
            )
            .AddTo(this);

    public void StartCatch(BagSize size)
    {
        col.enabled = false;
        handCollider.enabled = true;

        anim.catchSize.Int = (int)size;
        anim.catchTrigger.Fire();
    }

    public ClothSphereColliderPair GetPressTarget()
    {
        handCollider.enabled = false;
        col.enabled = true;
        headCollider.enabled = footCollider.enabled = true;
        return new ClothSphereColliderPair(headCollider, footCollider);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BagControl>() == null) return;

        col.enabled = false;

        anim.drop.Fire();

        randomWind.isWindActive = false;
    }
}
