using UnityEngine;

public class DebugEnemyGenerator : EnemyAutoGenerator
{
    [SerializeField] private EnemyType type = default;

    private EnemyData enemyData = default;

    protected override void Awake()
    {
        base.Awake();
        enemyData = ResourceLoader.Instance.enemyData;
        gameObject.SetActive(false);
    }

    protected void Start()
    {
        Vector3 pos = transform.position;
        WorldMap map = GameManager.Instance.worldMap;
        Init(gameObject, map.GetTile(pos), enemyData.Param((int)type));

        Activate();
    }
}
