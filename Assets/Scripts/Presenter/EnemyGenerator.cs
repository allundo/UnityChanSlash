using UnityEngine;
using System.Collections;

public class EnemyGenerator : MonoBehaviour
{
    [SerializeField] protected GameObject enemyPrefab = default;
    protected Transform pool;

    private void Start()
    {
        pool = gameObject.transform;
        StartCoroutine(SpawnLoop());
    }

    protected void Spawn()
    {
        foreach (Transform t in pool)
        {
            if (!t.gameObject.activeSelf)
            {
                t.GetComponent<EnemyCommander>().Respawn();
                return;
            }
        }
        Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity, pool);
    }

    public IEnumerator SpawnLoop()
    {
        while (true)
        {
            Spawn();
            yield return new WaitForSeconds(10);
        }
    }
}