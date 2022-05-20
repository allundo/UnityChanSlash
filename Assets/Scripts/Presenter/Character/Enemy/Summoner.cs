using UnityEngine;
using UniRx;
using System;
using System.Collections;
using System.Collections.Generic;

public class Summoner
{
    private GameManager gm;
    private IMobMapUtil map;

    private IDisposable summonDisposable = null;
    public bool IsSummoning => summonDisposable != null;
    public void StopSummoning()
    {
        summonDisposable?.Dispose();
        summonDisposable = null;
    }

    public Summoner(IMobMapUtil map)
    {
        gm = GameManager.Instance;
        this.map = map;
    }

    public IEnemyStatus Summon(EnemyType type, Pos pos, IDirection dir)
        => gm.PlaceEnemy(type, pos, dir, new EnemyStatus.ActivateOption(1.5f, true));

    public IEnemyStatus SummonRandom(Pos pos, IDirection dir)
        => gm.PlaceEnemyRandom(pos, dir, new EnemyStatus.ActivateOption(1.5f, true));

    public void SummonMulti(int count)
    {
        summonDisposable = Observable
            .FromCoroutine(() => SummonProcess(count))
            .IgnoreElements()
            .Subscribe(null, () => summonDisposable = null)
            .AddTo(map.transform);
    }

    private IEnumerator SummonProcess(int count)
    {
        var summoned = new List<Pos>();

        for (int i = 0; i < count; i++)
        {
            Pos pos = map.SearchSpaceNearBy(2, summoned);

            if (pos.IsNull) break;

            summoned.Add(pos);
            SummonRandom(pos, map.dir);
            yield return new WaitForEndOfFrame();
        }

        // Cool time 10 sec
        yield return new WaitForSeconds(10);
    }

}
