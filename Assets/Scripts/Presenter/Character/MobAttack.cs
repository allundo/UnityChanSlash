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
        if (swingSound != null)
        {
            swingSound.pitch = Pitch;
            swingSound.Play();
        }
        vfx?.Play();
    }

    public virtual void OnAttackFinished() { }

    public virtual void OnHitStart()
    {
        attackCollider.enabled = true;
    }

    public virtual void OnHitAttack(Collider collider)
    {
        MobReactor targetMob = collider.GetComponent<MobReactor>();

        if (null == targetMob) return;

        targetMob.OnDamage(status.Attack * attackMultiplier, status.dir);
    }

    public virtual void OnHitFinished()
    {
        attackCollider.enabled = false;
    }

    public Tween AttackSequence(float attackDuration)
    {
        return DOTween.Sequence()
            .Join(DOVirtual.DelayedCall(0, OnAttackStart, false))
            .Join(DOVirtual.DelayedCall(FrameToSec(startFrame), OnHitStart, false))
            .Join(DOVirtual.DelayedCall(FrameToSec(finishFrame), OnHitFinished, false))
            .Join(DOVirtual.DelayedCall(attackDuration, OnAttackFinished, false));
    }
}
