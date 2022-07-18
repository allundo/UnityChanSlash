using UnityEngine;
using UniRx;
using System;

public interface IGameManager
{
    WorldMap worldMap { get; }

    IObservable<Unit> ExitObservable { get; }

    void ActiveMessage(string message);
    void ActiveMessage(ActiveMessageData messageData);

    void DropStart();
    void Restart();
    void DebugStart();

    void Exit();
    void EnterStair(bool isDownStairs);

    void PlayVFX(VFXType type, Vector3 pos);
    void PlaySnd(SNDType type, Vector3 pos);
}
