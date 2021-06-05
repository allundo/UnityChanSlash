using UnityEngine;

[RequireComponent(typeof(MobStatus))]
public class MobAttack : MonoBehaviour
{
    [SerializeField] private Collider attackCollider = default;
    [SerializeField] private AudioSource swingSound = default;

    protected MobStatus status;

    protected virtual float Pitch => Random.Range(0.7f, 1.3f);

    protected virtual void Start()
    {
        status = GetComponent<MobStatus>();

        attackCollider.enabled = false;
    }

    public virtual void OnAttackStart()
    {
        attackCollider.enabled = true;
        if (swingSound != null)
        {
            swingSound.pitch = Pitch;
            swingSound.Play();
        }
    }

    public void OnHitAttack(Collider collider)
    {
        MobReactor targetMob = collider.GetComponent<MobReactor>();

        if (null == targetMob) return;

        targetMob.OnDamage(status.Attack, status.dir);
    }

    public void OnAttackFinished()
    {
        attackCollider.enabled = false;
    }
}