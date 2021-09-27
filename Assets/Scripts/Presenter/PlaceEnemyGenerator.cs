using UnityEngine;

public class PlaceEnemyGenerator : MonoBehaviour
{
    [SerializeField] private EnemyGenerator[] prefabEnemyGenerators = default;
    private GameObject enemyPool;

    public void Place(WorldMap map)
    {
        enemyPool = new GameObject("Enemy Pool");

        int division = (int)Mathf.Log(prefabEnemyGenerators.Length, 4);

        var regions = GetRegions(new Pos(0, 0), new Pos(map.Width - 1, map.Height - 1), division);

        for (int counter = 0; counter < prefabEnemyGenerators.Length; counter++)
        {
            (Pos leftTop, Pos rightBottom) region = regions[counter % regions.Length];

            int x = Random.Range(region.leftTop.x, region.rightBottom.x + 1);
            int y = Random.Range(region.leftTop.y, region.rightBottom.y + 1);

            Ground ground = map.GetGround(ref x, ref y);

            if (ground == null) continue;

            Instantiate(prefabEnemyGenerators[counter], map.WorldPos(x, y), Quaternion.identity).Init(enemyPool, ground);
        }
    }

    private (Pos leftTop, Pos rightBottom)[] GetRegions(Pos leftTop, Pos rightBottom, int division = 1, int offset = 0, (Pos, Pos)[] regions = null)
    {
        int numOfRegions = (int)Mathf.Pow(4, division);

        if (regions == null) regions = new (Pos, Pos)[numOfRegions];

        if (division == 0)
        {
            regions[offset] = (leftTop, rightBottom);
            return regions;
        }

        Pos center = (leftTop + rightBottom) / 2;
        int offsetUnit = numOfRegions / 4;

        GetRegions(leftTop, center, division - 1, offset, regions);
        GetRegions(new Pos(center.x + 1, leftTop.y), new Pos(rightBottom.x, center.y), division - 1, offset + offsetUnit, regions);
        GetRegions(new Pos(leftTop.x, center.y + 1), new Pos(center.x, rightBottom.y), division - 1, offset + offsetUnit * 2, regions);
        GetRegions(new Pos(center.x + 1, center.y + 1), rightBottom, division - 1, offset + offsetUnit * 3, regions);

        return regions;
    }
}