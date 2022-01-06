using UnityEngine;

public class DebugEnemyGenerator : EnemyAutoGenerator
{
    [SerializeField] private MobData enemyData = default;
    [SerializeField] private EnemyType type = default;

    protected override void Awake()
    {
        base.Awake();
        gameObject.SetActive(false);
    }

    protected override void Start()
    {
        base.Start();
        Vector3 pos = transform.position;
        WorldMap map = GameManager.Instance.worldMap;
        Init(gameObject, map.GetTile(pos), enemyData.Param((int)type));
    }
}
