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

    void DropStart();
    void Restart();
    void DebugStart();

    void Exit();
    void EnterStair(bool isDownStairs);

    void PlayVFX(VFXType type, Vector3 pos);
    void PlaySnd(SNDType type, Vector3 pos);
}
