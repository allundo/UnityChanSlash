using UnityEngine;

public class UnityChanResultReactor : MonoBehaviour
{
    private CapsuleCollider col;
    private ResultFaceAnimator anim;

    private bool isFalling = false;

    void Awake()
    {
        col = GetComponent<CapsuleCollider>();
        anim = GetComponent<ResultFaceAnimator>();

        isFalling = false;
        col.direction = 1;
        col.center = Vector3.up * 0.8f;
        col.isTrigger = true;
    }

    public void SetNormal() => anim.normal.Fire();
    public void SetSmile() => anim.smile.Fire();
    public void SetEyeClose() => anim.eyeClose.Fire();

    private void OnTriggerEnter(Collider other)
    {
        if (isFalling || other.GetComponent<BagControl>() == null) return;

        col.direction = 2;
        col.center = Vector3.zero;
        col.isTrigger = false;
        isFalling = true;

        anim.drop.Fire();
    }
}