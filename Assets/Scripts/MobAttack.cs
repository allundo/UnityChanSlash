using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MobStatus))]
public class MobAttack : MonoBehaviour
{
    [SerializeField] private Collider attackCollider = default;
    [SerializeField] private AudioSource swingSound = default;

    protected MobStatus status = default;
    private void Start()
    {
        status = GetComponent<MobStatus>();
        attackCollider.enabled = false;
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

        targetMob.Damage(status.Attack);
    }

    public void OnAttackFinished()
    {
        attackCollider.enabled = false;
    }
}