using DG.Tweening;
using System.Linq;
using System.Collections.Generic;
using UniRx;

public class GameInfo : SingletonMonoBehaviour<GameInfo>
{
    private static readonly int MAX_FLOOR = 10;
    private WorldMap[] maps;

    public int startActionID = 0;

    public WorldMap Map(int floor)
    {
#if UNITY_EDITOR

        if (maps[MAX_FLOOR] != null)
        {
            startActionID = 2;
            return maps[MAX_FLOOR];
        }

#endif

        if (floor > 0 && floor <= MAX_FLOOR)
        {
            return (maps[floor - 1] = maps[floor - 1] ?? new WorldMap());
        }

        return null;
    }

    protected override void Awake()
    {
        base.Awake();

        DOTween.SetTweensCapacity(500, 100);

        ClearMaps();

#if UNITY_EDITOR

        int[] matrix = new[]
        {
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            2, 0, 4, 1, 1, 1, 4, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 0, 2, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 0, 2, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 4, 2, 4, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 4, 0, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
        };

        var deadEndPos = new Dictionary<Pos, IDirection>()
        {
            {new Pos(5, 5), Direction.west},
            {new Pos(1, 1), Direction.north},
            {new Pos(1, 3), Direction.north},
            {new Pos(1, 5), Direction.north},
        };

        maps[MAX_FLOOR] = new WorldMap(new MapManager(matrix, 15, deadEndPos).SetStair());

#endif

    }

    public void ClearMaps()
    {
        maps = Enumerable.Repeat<WorldMap>(null, MAX_FLOOR + 1).ToArray();
    }
}
