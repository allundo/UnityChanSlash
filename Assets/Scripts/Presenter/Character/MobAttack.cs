using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider))]
public class MobAttack : MonoBehaviour
{
    [SerializeField] private AudioSource swingSound = default;
    [SerializeField] protected ParticleSystem vfx = default;

    [SerializeField] protected float attackMultiplier = 1.0f;
    [SerializeField] protected int startFrame = 0;
    [SerializeField] protected int finishFrame = 0;
    [SerializeField] protected int speed = 1;
    [SerializeField] protected float minPitch = 0.7f;
    [SerializeField] protected float maxPitch = 1.3f;
    [SerializeField] protected int frameRate = 30;

    private Collider attackCollider = default;
    private MobStatus status;

    protected virtual float Pitch => Random.Range(minPitch, maxPitch);

    protected virtual void Awake()
    {
        attackCollider = GetComponent<Collider>();
        status = GetComponentInParent<MobStatus>();

        attackCollider.enabled = false;
    }

    private float FrameToSec(int frame)
    {
        return (float)frame / (float)frameRate / (float)speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        OnHitAttack(other);
    }

    public virtual void OnAttackStart()
    {
        attackCollider.enabled = true;
        if (swingSound != null)
        {
            swingSound.pitch = Pitch;
            swingSound.Play();
        }
        vfx?.Play();
    }

    public void OnHitAttack(Collider collider)
    {
        MobReactor targetMob = collider.GetComponent<MobReactor>();

        if (null == targetMob) return;

        targetMob.OnDamage(status.Attack * attackMultiplier, status.dir);
    }

    public void OnAttackFinished()
    {
        attackCollider.enabled = false;
    }

    public Tween SetAttack()
    {
        return DOTween.Sequence()
            .Join(DOVirtual.DelayedCall(FrameToSec(startFrame), OnAttackStart))
            .Join(DOVirtual.DelayedCall(FrameToSec(finishFrame), OnAttackFinished))
            .Play();
    }
}