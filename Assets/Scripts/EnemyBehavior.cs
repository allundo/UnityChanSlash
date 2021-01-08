using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyStatus))]
public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] private LayerMask raycastLayerMask = default;
    private NavMeshAgent agent;
    private RaycastHit[] raycastHits = new RaycastHit[10];

    private EnemyStatus status;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        status = GetComponent<EnemyStatus>();
    }

    public void OnDetectObject(Collider collider)
    {
        if (!status.IsMovable)
        {
            agent.isStopped = true;
            return;
        }


        if (collider.CompareTag("Player"))
        {
            Vector3 positionDiff = collider.transform.position - transform.position;

            // y 成分は考えない
            positionDiff.y = 0.0f;
            float distance = positionDiff.magnitude;
            Vector3 direction = positionDiff.normalized;

            int hitCount = Physics.RaycastNonAlloc(transform.position, direction, raycastHits, distance, raycastLayerMask);

            float minDistance = 10000.0f;
            string minTag = "Player";
            for (int i = 0; i < hitCount; i++)
            {
                if (raycastHits[i].distance < minDistance)
                {
                    minDistance = raycastHits[i].distance;
                    minTag = raycastHits[i].collider.tag;
                }
            }

            agent.isStopped = minTag != "Player";

            if (!agent.isStopped)
            {
                agent.destination = collider.transform.position;
            }
        }
    }
}
