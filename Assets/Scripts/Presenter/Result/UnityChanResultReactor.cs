using UnityEngine;
using UniRx;
using System;

public class UnityChanResultReactor : MonoBehaviour
{
    [SerializeField] private SphereCollider headCollider = default;
    [SerializeField] private SphereCollider footCollider = default;
    [SerializeField] private SphereCollider handCollider = default;
    [SerializeField] private AudioSource pressSnd = default;

    private CapsuleCollider col;
    private ResultFaceAnimator anim;
    private SpringManager springManager;

    public ClothSphereColliderPair sphereColliderPair { get; private set; }

    public IObservable<Unit> ScreenOut => screenOutSubject;
    private ISubject<Unit> screenOutSubject = new Subject<Unit>();

    public IObservable<Unit> ReHold => reHoldSubject;
    private ISubject<Unit> reHoldSubject = new Subject<Unit>();

    void Awake()
    {
        col = GetComponent<CapsuleCollider>();
        anim = GetComponent<ResultFaceAnimator>();
        springManager = GetComponent<SpringManager>();

        headCollider.enabled = footCollider.enabled = false;
        sphereColliderPair = new ClothSphereColliderPair(headCollider, footCollider);
    }

    public void SetNormal() => anim.normal.Fire();
    public void SetSmile() => anim.smile.Fire();
    public void SetEyeClose() => anim.eyeClose.Fire();
    public void SetSurprise() => anim.surprise.Fire();
    public void SetDisattract() => anim.disattract.Fire();

    public void UpHold() => reHoldSubject.OnNext(Unit.Default);
    public void Carrying() => reHoldSubject.OnCompleted();

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
        // If the object has Rigidbody, OnTriggerEnter() is called also when collision of child objects is detected.
        if (!col.enabled || other.GetComponent<YenBag>() == null) return;

        col.enabled = false;

        anim.drop.Fire();

        springManager.Pause();

        pressSnd.PlayEx();
    }
}
