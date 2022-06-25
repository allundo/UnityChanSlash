using UnityEngine;
using UniRx;
using System;

public interface IGameManager
{
    WorldMap worldMap { get; }

    Pos PlayerPos { get; }
    IDirection PlayerDir { get; }
    bool IsPlayerHavingKeyBlade { get; }

    bool IsOnPlayer(Pos pos);
    bool IsOnPlayer(int x, int y);
    bool IsOnPlayerTile(Pos pos);
    bool IsOnPlayerTile(int x, int y);

    IObservable<Unit> ExitObservable { get; }

    bool isPaused { get; }
    void Pause(bool isHideUIs = false);
    void Resume(bool isShowUIs = true);
    void TimeScale(float scale = 5f);

    void DropStart();
    void Restart();
    void DebugStart();

    void EraseAllEnemies();
    IEnemyStatus PlaceWitch(Pos pos, IDirection dir, float waitFrames = 120f);
    IEnemyStatus PlaceEnemy(EnemyType type, Pos pos, IDirection dir, EnemyStatus.ActivateOption option, float life = 0f);
    IEnemyStatus PlaceEnemyRandom(Pos pos, IDirection dir, EnemyStatus.ActivateOption option, float life = 0f);

    BulletGenerator GetBulletGenerator(BulletType type);

    void SpawnLight(Vector3 pos);
    void DistributeLight(Vector3 pos, float range);

    void Exit();
    void EnterStair(bool isDownStairs);

    void PlayVFX(VFXType type, Vector3 pos);
    void PlaySnd(SNDType type, Vector3 pos);
}
