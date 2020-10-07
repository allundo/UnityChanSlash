using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MobStatus))]
public class MobAttack : MonoBehaviour
{
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private Collider attackCollider = default;
    [SerializeField] private AudioSource swingSound = default;

    private MobStatus status;

    private void Start()
    {
        status = GetComponent<MobStatus>();
        attackCollider.enabled = false;
    }

    public void AttackIfPossible()
    {
        if (!status.IsAttackable) return;

        status.GoToAttackStateIfPossible();
    }

    public void OnAttackRangeEnter(Collider collider)
    {
        AttackIfPossible();
    }

    public void OnAttackStart()
    {
        attackCollider.enabled = true;
        if (swingSound != null)
        {
            swingSound.pitch = Random.Range(0.7f, 1.3f);
            swingSound.Play();
        }
    }

    public void OnHitAttack(Collider collider)
    {
        MobStatus targetMob = collider.GetComponent<MobStatus>();

        if (null == targetMob) return;

        targetMob.Damage(1);
    }

    public void OnAttackFinished()
    {
        attackCollider.enabled = false;
        StartCoroutine(CooldownCoroutine());
    }

    private IEnumerator CooldownCoroutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        status.GoToNormalStateIfPossible();
    }
}