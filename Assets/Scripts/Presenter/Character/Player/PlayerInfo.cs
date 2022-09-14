using UnityEngine;

[RequireComponent(typeof(PlayerMapUtil))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerInfo : SingletonMonoBehaviour<PlayerInfo>
{
    private PlayerMapUtil map;
    private PlayerInput input;
    private PlayerStatus status;

    protected override void Awake()
    {
        base.Awake();
        map = GetComponent<PlayerMapUtil>();
        input = GetComponent<PlayerInput>();
        status = GetComponent<PlayerStatus>();
    }

    public Pos PlayerPos => map.onTilePos;
    public IDirection PlayerDir => map.dir;
    public bool IsPlayerHavingKeyBlade => input.GetItemInventory.hasKeyBlade();

    public bool IsOnPlayer(Pos pos) => gameObject.activeSelf && !map.isInPit && PlayerPos == pos;
    public bool IsOnPlayer(int x, int y) => IsOnPlayer(new Pos(x, y));
    public bool IsOnPlayerTile(Pos pos) => gameObject.activeSelf && !map.isInPit && map.onTilePos == pos;
    public bool IsOnPlayerTile(int x, int y) => IsOnPlayerTile(new Pos(x, y));

    public DataStoreAgent.MobData ExportRespawnData()
    {
        return new DataStoreAgent.MobData(PlayerPos, status);
    }

    public void ImportRespawnData(DataStoreAgent.MobData data)
    {
        status.SetPosition(data.kvPosDir);
        input.SetInputVisible();
    }

    public void ImportStatusData(MobStoreData data)
        => status.SetStatusData(data.life, data.isIced, data.isHidden);
}
