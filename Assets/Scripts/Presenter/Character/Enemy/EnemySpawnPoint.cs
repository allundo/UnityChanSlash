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

    void Start()
    {
        enemyAutoGenerator.gameObject.SetActive(false);
    }

    public EnemySpawnPoint Init(GameObject enemyPool, ITile tile, MobParam param)
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
}
