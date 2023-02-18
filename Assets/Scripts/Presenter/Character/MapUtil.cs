using UnityEngine;

/// <summary>
/// Map の Tile の状態を更新するメソッドを公開 <br>
/// Map の状態とキャラクターの位置・向きに応じた情報を提供するメソッドを公開
/// </summary>
[RequireComponent(typeof(Status))]
public class MapUtil : MonoBehaviour, IMapUtil
{
    protected IStatus status;
    protected WorldMap map;
    protected Transform tf;

    public virtual IDirection dir { get { return status.dir; } set { status.SetDir(value); } }

    /// <summary>
    /// Tile position of destination for moving
    /// </summary>
    public Pos onTilePos { get; protected set; }

    public static readonly float TILE_UNIT = Constants.TILE_UNIT;

    protected virtual void Awake()
    {
        this.tf = transform;
        status = GetComponent<Status>();
    }

    public virtual void OnActive()
    {
        this.map = GameManager.Instance.worldMap;
        SetObjectOn();
    }

    public Vector3 WorldPos(Pos pos) => map.WorldPos(pos);

    public virtual void TurnLeft()
    {
        dir = dir.Left;
    }

    public virtual void TurnRight()
    {
        dir = dir.Right;
    }

    public virtual void TurnBack()
    {
        dir = dir.Backward;
    }

    public Vector3 GetForwardVector(float distance = 1f) => dir.LookAt * TILE_UNIT * distance;
    public Vector3 GetBackwardVector(float distance = 1f) => -dir.LookAt * TILE_UNIT * distance;
    public Vector3 GetRightVector(float distance = 1f) => Quaternion.Euler(0, 90, 0) * dir.LookAt * TILE_UNIT * distance;
    public Vector3 GetLeftVector(float distance = 1f) => Quaternion.Euler(0, -90, 0) * dir.LookAt * TILE_UNIT * distance;

    public ITile GetTile(Pos destPos) => map.GetTile(destPos);
    public ITile OnTile => map.GetTile(onTilePos);
    public ITile ForwardTile => map.GetTile(GetForward);
    public ITile BackwardTile => map.GetTile(GetBackward);
    public ITile RightTile => map.GetTile(GetRight);
    public ITile LeftTile => map.GetTile(GetLeft);
    public ITile JumpTile => map.GetTile(GetJump);

    public bool IsObjectOn(Pos destPos) => map.GetTile(destPos).IsCharacterOn;
    public bool IsViewable(Pos destPos) => map.GetTile(destPos).IsViewOpen;

    public Vector3 CurrentVec3Pos => tf.position;
    public Vector3 DestVec => WorldPos(onTilePos) - tf.position;
    public Vector3 DestVec3Pos => WorldPos(onTilePos);

    public Pos GetForward => dir.GetForward(onTilePos);
    public Pos GetLeft => dir.GetLeft(onTilePos);
    public Pos GetRight => dir.GetRight(onTilePos);
    public Pos GetBackward => dir.GetBackward(onTilePos);
    public Pos GetJump => dir.GetForward(dir.GetForward(onTilePos));

    public static bool IsOnPlayer(Pos destPos) => PlayerInfo.Instance.IsOnPlayer(destPos);

    public bool IsPlayerFound() => IsPlayerFound(onTilePos);
    /// <summary>
    /// 指定した地点から前方に向けてプレイヤーがいないか再帰的にチェック<br>
    /// 壁などの進めないマスに到達したら探索終了
    /// </summary>
    /// <param name="pos">現在位置を指定することで、正面1マス先から探索を開始する</param>
    /// <returns>プレイヤーを見つけたらtrue</returns>
    public bool IsPlayerFound(Pos pos)
    {
        Pos forward = dir.GetForward(pos);

        return IsOnPlayer(forward)
            ? true
            : !IsViewable(forward)
                ? false
                : IsPlayerFound(forward);
    }

    public bool IsPlayerForward => MapUtil.IsOnPlayer(GetForward);
    public bool IsPlayerLeft => MapUtil.IsOnPlayer(GetLeft);
    public bool IsPlayerRight => MapUtil.IsOnPlayer(GetRight);
    public bool IsPlayerBackward => MapUtil.IsOnPlayer(GetBackward);

    /// <summary>
    /// Set current on tile and IsObjectOn flag to the Tile currently on
    /// </summary>
    /// <returns>Current map Pos</returns>
    public Pos SetObjectOn() => SetObjectOn(transform.position);

    /// <summary>
    /// Set current on tile and IsObjectOn flag to the Tile specified by Vector3 position
    /// </summary>
    /// <param name="destPos">Vector3 position of destination Tile</param>
    /// <returns>destPos</returns>
    public Pos SetObjectOn(Vector3 destPos) => SetObjectOn(map.MapPos(destPos));

    /// <summary>
    /// Set current on tile and IsObjectOn flag to the Tile specified by Pos unit
    /// </summary>
    /// <param name="destPos">Tile map position of destination</param>
    /// <returns>destPos</returns>
    public virtual Pos SetObjectOn(Pos destPos)
    {
        onTilePos = destPos;
        return destPos;
    }

    /// <summary>
    /// Apply RemoveObjectOn at current Tile and SetObjectOn to the dest Pos.
    /// </summary>
    public Pos MoveObjectOn(Vector3 destPos) => MoveObjectOn(map.MapPos(destPos));

    public virtual Pos MoveObjectOn(Pos destPos) => SetObjectOn(destPos);
}
