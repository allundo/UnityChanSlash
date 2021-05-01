using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MobStatus))]
[RequireComponent(typeof(MobCommander))]
public class MobAttack : MonoBehaviour
{
    [SerializeField] private Collider attackCollider = default;
    [SerializeField] private AudioSource swingSound = default;

    protected MobCommander commander;

    protected Direction dir => commander.dir;
    protected Animator anim => commander.anim;

    protected virtual float Pitch => Random.Range(0.7f, 1.3f);

    protected MobStatus status = default;
    protected virtual void Start()
    {
        status = GetComponent<MobStatus>();
        commander = GetComponent<MobCommander>();

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
        MobStatus targetMob = collider.GetComponent<MobStatus>();

        if (null == targetMob) return;

        targetMob.Damage(status.Attack, dir);
    }

    public void OnAttackFinished()
    {
        attackCollider.enabled = false;
    }
}