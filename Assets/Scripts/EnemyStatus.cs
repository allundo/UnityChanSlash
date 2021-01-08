using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyStatus : MobStatus
{
    [SerializeField] private float damageCooldown = 10.0f;
    private NavMeshAgent agent;

    protected override void Start()
    {
        base.Start();

        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        anim.SetFloat("MoveSpeed", agent.velocity.magnitude);
    }

    protected override void OnDie()
    {
        base.OnDie();
        StartCoroutine(DestoryCoroutine());
    }

    protected override void OnDamage()
    {
        Vector3 back = transform.TransformDirection(Vector3.back) * 12.0f;
        transform.DOMove(back, damageCooldown * 0.5f).Play();
        agent.isStopped = true;
        StartCoroutine(CooldownCoroutine());
    }

    private IEnumerator DestoryCoroutine()
    {
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }

    private IEnumerator CooldownCoroutine()
    {
        yield return new WaitForSeconds(damageCooldown);
        agent.isStopped = false;
        GoToNormalStateIfPossible();
    }
}
