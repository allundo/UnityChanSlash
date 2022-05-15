using System.Collections.Generic;

public class Summoner
{
    private GameManager gm;
    private IMobMapUtil map;

    public Summoner(IMobMapUtil map)
    {
        gm = GameManager.Instance;
        this.map = map;
    }

    public IEnemyStatus Summon(EnemyType type, Pos pos, IDirection dir)
        => gm.PlaceEnemy(type, pos, dir, new EnemyStatus.ActivateOption(2f, true));

    public IEnemyStatus SummonRandom(Pos pos, IDirection dir)
        => gm.PlaceEnemyRandom(pos, dir, new EnemyStatus.ActivateOption(2f, true));

    public void SummonMulti(int count)
    {
        var summoned = new List<Pos>();

        for (int i = 0; i < count; i++)
        {
            Pos pos = map.SearchSpaceNearBy(2, summoned);

            if (pos.IsNull) return;

            summoned.Add(pos);
            SummonRandom(pos, map.dir);
        }
    }
}
