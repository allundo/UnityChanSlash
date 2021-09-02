using DG.Tweening;
using System.Linq;

public class GameInfo : SingletonMonoBehaviour<GameInfo>
{
    private static readonly int MAX_FLOOR = 10;
    private WorldMap[] maps = Enumerable.Repeat<WorldMap>(null, MAX_FLOOR).ToArray();

    public WorldMap Map(int floor)
    {
        if (floor > 0 && floor <= maps.Length)
        {
            return (maps[floor - 1] = maps[floor - 1] ?? new WorldMap());
        }

        return null;
    }

    protected override void Awake()
    {
        base.Awake();

        DOTween.SetTweensCapacity(500, 100);
    }
}
