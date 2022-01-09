using UnityEngine;

public class DebugEnemyGenerator : EnemyAutoGenerator
{
    [SerializeField] private EnemyData enemyData = default;
    [SerializeField] private EnemyType type = default;

    protected override void Awake()
    {
        base.Awake();
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
