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

        // Activate generator when the player is detected nearby
        if (enemy != null)
        {
            enemy.FadeOutOnDead();
        }
    }
}