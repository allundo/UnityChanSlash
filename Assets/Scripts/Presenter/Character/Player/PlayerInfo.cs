using UnityEngine;
using System;
using UniRx;

[RequireComponent(typeof(PlayerMapUtil))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerStatus))]
[RequireComponent(typeof(PlayerAnimator))]
public class PlayerInfo : SingletonMonoBehaviour<PlayerInfo>
{
    private PlayerMapUtil mapUtil;
    private PlayerInput input;
    private PlayerStatus status;
    private PlayerEffect effect;
    private PlayerAnimator anim;

    protected override void Awake()
    {
        base.Awake();
        mapUtil = GetComponent<PlayerMapUtil>();
        input = GetComponent<PlayerInput>();
        status = GetComponent<PlayerStatus>();
        effect = GetComponent<PlayerEffect>();
        anim = GetComponent<PlayerAnimator>();
    }

    public Pos Pos => mapUtil.onTilePos;
    public Vector3 Vec3Pos => mapUtil.CurrentVec3Pos;
    public IDirection Dir => mapUtil.dir;
    public IObservable<IDirection> DirObservable => mapUtil.Dir.Where(dir => dir != null);

    public bool IsOnPlayer(Pos pos) => gameObject.activeSelf && !mapUtil.isInPit && Pos == pos;
    public bool IsOnPlayer(int x, int y) => IsOnPlayer(new Pos(x, y));
    public bool IsOnPlayerTile(Pos pos) => gameObject.activeSelf && !mapUtil.isInPit && mapUtil.onTilePos == pos;
    public bool IsOnPlayerTile(int x, int y) => IsOnPlayerTile(new Pos(x, y));

    public DataStoreAgent.PlayerData ExportRespawnData() => ExportRespawnData(Pos);
    public DataStoreAgent.PlayerData ExportRespawnData(Pos playerPos)
    {
        var cmd = input.currentCommand;

        var exitState = ExitState.Normal;

        if (cmd is PlayerJump && cmd.RemainingTimeScale > 0.25f) exitState = ExitState.Jump;

        if (mapUtil.isInPit)
        {
            // Player has already fell into pit if Idling(cmd is null), Turn LR in the pit or failed to escape from the pit by Jump.
            exitState = (cmd == null || cmd is PlayerTurnL || cmd is PlayerTurnR || cmd is PlayerPitJump || cmd is PlayerWakeUp) ? ExitState.InPit : ExitState.PitFall;
        }

        if (cmd is PlayerIcedCommand)
        {
            exitState = ExitState.Iced;
            // TODO: Store remaining IcedCommand duration.
        }

        if (cmd is PlayerIcedFall) exitState = ExitState.IcedFall;
        if (cmd is PlayerIcedPitFall) exitState = ExitState.IcedPitFall;

        return new DataStoreAgent.PlayerData(playerPos, status, exitState);
    }

    public void ImportRespawnData(DataStoreAgent.PlayerData data, WorldMap map)
    {
        mapUtil.SetStartPos(map, data.kvPosDir);
        input.SetInputVisible();
    }

    public void RestorePlayerStatus(DataStoreAgent.PlayerData data)
    {
        status.SetStatusData(data.life, data.level, data.exp, data.isHidden, data.counter, data.levelGainType);

        var state = data.exitState;

        switch (state)
        {
            case ExitState.Iced:
                input.InputIced(data.icingFrames);
                return;

            case ExitState.InPit:
                transform.position -= Vector3.up * Constants.TILE_UNIT;
                input.SetInputVisible(false);
                input.SetInputVisible(true, false);
                return;

            case ExitState.Jump:
            case ExitState.IcedFall:
            case ExitState.IcedPitFall:
                Vector3 landingVec = data.dir.LookAt * 0.5f + Vector3.up * -0.75f;
                transform.position -= landingVec;

                if (state == ExitState.Jump)
                {
                    input.SetInputVisible();
                    input.EnqueueLanding(landingVec);
                    return;
                }

                input.InterruptIcedFall(data.icingFrames, state == ExitState.IcedPitFall);
                return;

            case ExitState.PitFall:
                // Spawn only and then fires pit drop event
                return;

            default:
                input.SetInputVisible();
                return;
        }
    }

    // ## FOR DEBUG (begin)
    public void DebugSetLevel(int level)
        => status.InitParam(Resources.Load<PlayerData>("DataAssets/Character/PlayerData").Param(1), new MobStatusStoreData(level));
    // ## FOR DEBUG (end)
}
