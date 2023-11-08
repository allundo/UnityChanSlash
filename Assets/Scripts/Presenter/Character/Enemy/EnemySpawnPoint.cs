using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemySpawnPoint : MonoBehaviour
{
    [SerializeField] protected EnemyAutoGenerator enemyAutoGenerator;

    protected Collider detectPlayer;

    void Awake()
    {
        detectPlayer = GetComponent<Collider>();
    }

    public EnemySpawnPoint Init(GameObject enemyPool, ITile tile, EnemyParam param)
    {
        enemyAutoGenerator.Init(enemyPool, tile, param);
        return this;
    }

    public void OnTriggerEnter(Collider other)
    {
        // Activate generator when the player is detected nearby
        if (other.GetComponent<EnemySpawnController>() != null)
        {
            enemyAutoGenerator.Activate();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        // Inactivate generator when the player leaves
        if (other.GetComponent<EnemySpawnController>() != null)
        {
            enemyAutoGenerator.Inactivate();
        }
    }

    public void Enable()
    {
        detectPlayer.enabled = true;
    }

    public void Disable()
    {
        detectPlayer.enabled = false;
        enemyAutoGenerator.Inactivate();
    }
}
