using UnityEngine;
using UniRx;
using System;

public interface IGameManager
{
    WorldMap worldMap { get; }

    IObservable<Unit> ExitObservable { get; }

    void DropStart();
    void Restart();
    void DebugStart();
    void LoadDataStart();

    void RedrawPlates();
    void DestructDoor(Vector3 pos, IDirection forceDir);
    void PitDropFX(Vector3 pos);

    DataStoreAgent.EventData[] ExportEventData();
    int GetCurrentEvent();
    void ImportRespawnData(DataStoreAgent.SaveData import);

    void Exit();
    void EnterStair(bool isDownStairs);

    void PlayVFX(VFXType type, Vector3 pos);
    void PlaySnd(SNDType type, Vector3 pos);
}
