using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemySpawnController : MonoBehaviour
{
    protected Collider detectCollider;

    void Awake()
    {
        detectCollider = GetComponent<Collider>();
    }

    public void OnTriggerEnter(Collider other) { }

    public void OnTriggerExit(Collider other)
    {
        EnemyReactor enemy = other.GetComponent<EnemyReactor>();

        // Inactivate the enemy getting out of player's view range
        if (enemy != null)
        {
            enemy.OnOutOfView();
        }
    }
}
