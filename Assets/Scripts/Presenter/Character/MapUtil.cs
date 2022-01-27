using UnityEngine;

/// <summary>
/// Map の Tile の状態を更新するメソッドを公開 <br>
/// Map の状態とキャラクターの位置・向きに応じた情報を提供するメソッドを公開
/// </summary>
[RequireComponent(typeof(MobStatus))]
public class MapUtil : MonoBehaviour, IMapUtil
{
    protected MobStatus status;
    protected WorldMap map;
    protected Transform tf;

    public IDirection dir { get { return status.dir; } set { status.SetDir(value); } }

    /// <summary>
    /// Tile position of destination for moving
    /// </summary>
    protected Pos onTilePos;

    public static readonly float TILE_UNIT = Constants.TILE_UNIT;

    protected virtual void Awake()
    {
        this.map = GameManager.Instance.worldMap;
        this.tf = transform;
        status = GetComponent<MobStatus>();
    }

    public virtual void OnActive() => SetObjectOn();

    public virtual void ResetTile() => RemoveObjectOn();

    public Vector3 WorldPos(Pos pos) => map.WorldPos(pos);

    public virtual void TurnLeft()
    {
        dir = dir.Left;
    }

    public virtual void TurnRight()
    {
        dir = dir.Right;
    }

    public Vector3 GetForwardVector(float distance = 1f) => dir.LookAt * TILE_UNIT * distance;
    public Vector3 GetBackwardVector(float distance = 1f) => -dir.LookAt * TILE_UNIT * distance;
    public Vector3 GetRightVector(float distance = 1f) => Quaternion.Euler(0, 90, 0) * dir.LookAt * TILE_UNIT * distance;
    public Vector3 GetLeftVector(float distance = 1f) => Quaternion.Euler(0, -90, 0) * dir.LookAt * TILE_UNIT * distance;

    public ITile GetTile(Pos destPos) => map.GetTile(destPos);
    public ITile ForwardTile => map.GetTile(GetForward);
    public ITile BackwardTile => map.GetTile(GetBackward);
    public ITile RightTile => map.GetTile(GetRight);
    public ITile LeftTile => map.GetTile(GetLeft);
    public ITile JumpTile => map.GetTile(GetJump);

    public bool IsObjectOn(Pos destPos) => map.GetTile(destPos).IsCharacterOn;
    public bool IsMovable(Pos destPos, IDirection dir = null) => map.GetTile(destPos).IsEnterable(dir);
    public bool IsLeapable(Pos destPos) => map.GetTile(destPos).IsLeapable;

    public Vector3 CurrentVec3Pos => tf.position;
    public Pos CurrentPos => onTilePos;
    public Vector3 DestVec => WorldPos(onTilePos) - tf.position;

    public Pos GetForward => dir.GetForward(onTilePos);
    public Pos GetLeft => dir.GetLeft(onTilePos);
    public Pos GetRight => dir.GetRight(onTilePos);
    public Pos GetBackward => dir.GetBackward(onTilePos);
    public Pos GetJump => dir.GetForward(dir.GetForward(onTilePos));

    public virtual bool IsForwardMovable => IsMovable(dir.GetForward(onTilePos));
    public bool IsForwardLeapable => IsLeapable(dir.GetForward(onTilePos));
    public virtual bool IsBackwardMovable => IsMovable(dir.GetBackward(onTilePos));
    public virtual bool IsLeftMovable => IsMovable(dir.GetLeft(onTilePos));
    public virtual bool IsRightMovable => IsMovable(dir.GetRight(onTilePos));
    public virtual bool IsJumpable => IsForwardLeapable && IsMovable(GetJump);

    public static bool IsOnPlayer(Pos destPos) => GameManager.Instance.IsOnPlayer(destPos);

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
            : !IsMovable(forward)
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
    public Pos SetObjectOn() => SetObjectOn(tf.position);

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
        if (status.IsOnGround) map.GetTile(destPos).OnCharacterDest = status;
        onTilePos = destPos;
        return destPos;
    }

    /// <summary>
    /// Set IsObjectOn flag FALSE to the Tile currently on
    /// </summary>
    public void RemoveObjectOn() => RemoveObjectOn(onTilePos);

    /// <summary>
    /// Set IsObjectOn flag FALSE to the Tile specified by Vector3 position
    /// </summary>
    /// <param name="pos">Vector3 Tile position</param>
    public void RemoveObjectOn(Vector3 pos) => RemoveObjectOn(map.MapPos(pos));

    /// <summary>
    /// Set IsObjectOn flag FALSE to the Tile specified by Vector3 position
    /// </summary>
    /// <param name="pos">Pos unit Tile position</param>
    public void RemoveObjectOn(Pos pos)
    {
        if (status.IsOnGround) map.GetTile(pos).OnCharacterDest = null;
    }

    /// <summary>
    /// Apply RemoveObjectOn at current Tile and SetObjectOn to the dest Pos.
    /// </summary>
    public Pos MoveObjectOn(Vector3 destPos) => MoveObjectOn(map.MapPos(destPos));

    /// <summary>
    /// Apply RemoveObjectOn at current Tile and SetObjectOn to the dest Pos.
    /// </summary>
    public Pos MoveObjectOn(Pos destPos)
    {
        RemoveObjectOn();
        return SetObjectOn(destPos);
    }
}
