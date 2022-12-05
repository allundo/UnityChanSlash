using UnityEngine;

[RequireComponent(typeof(PlayerMapUtil))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerStatus))]
[RequireComponent(typeof(PlayerAnimator))]
public class PlayerInfo : SingletonMonoBehaviour<PlayerInfo>
{
    private PlayerMapUtil map;
    private PlayerInput input;
    private PlayerStatus status;
    private PlayerEffect effect;
    private PlayerAnimator anim;

    protected override void Awake()
    {
        base.Awake();
        map = GetComponent<PlayerMapUtil>();
        input = GetComponent<PlayerInput>();
        status = GetComponent<PlayerStatus>();
        effect = GetComponent<PlayerEffect>();
        anim = GetComponent<PlayerAnimator>();
    }

    public Pos PlayerPos => map.onTilePos;
    public Vector3 PlayerVec3Pos => map.CurrentVec3Pos;
    public IDirection PlayerDir => map.dir;
    public bool IsPlayerHavingKeyBlade => input.GetItemInventory.hasKeyBlade();

    public bool IsOnPlayer(Pos pos) => gameObject.activeSelf && !map.isInPit && PlayerPos == pos;
    public bool IsOnPlayer(int x, int y) => IsOnPlayer(new Pos(x, y));
    public bool IsOnPlayerTile(Pos pos) => gameObject.activeSelf && !map.isInPit && map.onTilePos == pos;
    public bool IsOnPlayerTile(int x, int y) => IsOnPlayerTile(new Pos(x, y));

    public DataStoreAgent.PlayerData ExportRespawnData()
    {
        var cmd = input.currentCommand;

        var exitState = ExitState.Normal;

        if (cmd is PlayerJump && cmd.RemainingTimeScale > 0.25f) exitState = ExitState.Jump;

        if (map.isInPit)
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

        return new DataStoreAgent.PlayerData(PlayerPos, status, exitState);
    }

    public DataStoreAgent.ItemInfo[] ExportInventoryItems()
    {
        return input.GetItemInventory.ExportInventoryItems();
    }

    public void ImportRespawnData(DataStoreAgent.PlayerData data)
    {
        status.SetPosition(data.kvPosDir);
        input.SetInputVisible();
    }

    public void ImportInventoryItems(DataStoreAgent.ItemInfo[] items)
    {
        input.GetItemInventory.ImportInventoryItems(items);

        // TODO: Restore equipments from item data.
    }

    public void RestorePlayerStatus(DataStoreAgent.PlayerData data)
    {
        status.SetStatusData(data.life, data.isHidden);

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
}
