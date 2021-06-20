using UnityEngine;

/// <summary>
/// Map の Tile の状態を更新するメソッドを公開 <br>
/// Map の状態とキャラクターの位置・向きに応じた情報を提供するメソッドを公開
/// </summary>
public class MapUtil : MonoBehaviour
{
    protected WorldMap map;
    private Transform tf;
    public Direction dir { get; protected set; }
    protected Pos onTilePos;
    public static readonly float TILE_UNIT = 2.5f;

    private static readonly Vector3 defaultPos = new Vector3(-50, 0, 50);
    private static readonly Direction defaultDir = new North();

    protected virtual void Awake()
    {
        this.map = GameManager.Instance.worldMap;
        this.tf = transform;
    }

    public virtual void SetPosition(bool IsOnCharactor = true) { SetPosition(defaultPos, defaultDir, IsOnCharactor); }
    public void SetPosition(Vector3 pos, Direction dir = null, bool IsOnCharactor = true)
    {
        this.tf.position = pos;

        this.dir = dir ?? MapUtil.defaultDir;
        tf.LookAt(pos + this.dir.LookAt);

        if (IsOnCharactor) SetOnCharactor();
    }

    public virtual void TurnLeft()
    {
        dir = dir.Left;
    }

    public virtual void TurnRight()
    {
        dir = dir.Right;
    }

    public Vector3 GetForwardVector(int distance = 1) => dir.LookAt * TILE_UNIT * distance;
    public Vector3 GetBackwardVector(int distance = 1) => -dir.LookAt * TILE_UNIT * distance;
    public Vector3 GetRightVector(int distance = 1) => Quaternion.Euler(0, 90, 0) * dir.LookAt * TILE_UNIT * distance;
    public Vector3 GetLeftVector(int distance = 1) => Quaternion.Euler(0, -90, 0) * dir.LookAt * TILE_UNIT * distance;

    public Tile ForwardTile => map.GetTile(dir.GetForward(onTilePos));
    public Tile BackwardTile => map.GetTile(dir.GetBackward(onTilePos));
    public Tile RightTile => map.GetTile(dir.GetRight(onTilePos));
    public Tile LeftTile => map.GetTile(dir.GetLeft(onTilePos));
    public Tile JumpTile => map.GetTile(dir.GetForward(dir.GetForward(onTilePos)));

    public bool IsCharactorOn(Pos destPos) => map.GetTile(destPos).IsCharactorOn;
    public bool IsMovable(Pos destPos, Direction dir = null) => map.GetTile(destPos).IsEnterable(dir);
    public bool IsLeapable(Pos destPos) => map.GetTile(destPos).IsLeapable();

    public Vector3 CurrentVec3Pos => tf.position;
    public Pos CurrentPos => onTilePos;
    public Pos GetForward => dir.GetForward(onTilePos);
    public Pos GetLeft => dir.GetLeft(onTilePos);
    public Pos GetRight => dir.GetRight(onTilePos);
    public Pos GetBackward => dir.GetBackward(onTilePos);

    public virtual bool IsForwardMovable => IsMovable(dir.GetForward(onTilePos));
    public bool IsForwardLeapable => IsLeapable(dir.GetForward(onTilePos));
    public virtual bool IsBackwardMovable => IsMovable(dir.GetBackward(onTilePos));
    public virtual bool IsLeftMovable => IsMovable(dir.GetLeft(onTilePos));
    public virtual bool IsRightMovable => IsMovable(dir.GetRight(onTilePos));
    public virtual bool IsJumpable => IsForwardLeapable && IsMovable(dir.GetForward(dir.GetForward(onTilePos)));

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
    /// Set IsCharactorOn flag TRUE to the Tile currently on
    /// </summary>
    public void SetOnCharactor() { SetOnCharactor(tf.position); }

    /// <summary>
    /// Set IsCharactorOn flag TRUE to the Tile specified by Vector3 position
    /// </summary>
    /// <param name="pos">Vector3 Tile position</param>
    public void SetOnCharactor(Vector3 pos) { SetOnCharactor(map.MapPos(pos)); }

    /// <summary>
    /// Set IsCharactorOn flag TRUE to the Tile specified by Pos unit
    /// </summary>
    /// <param name="pos">Pos unit Tile position</param>
    public void SetOnCharactor(Pos pos)
    {
        map.GetTile(pos).IsCharactorOn = true;
        onTilePos = pos;
    }

    /// <summary>
    /// Set IsCharactorOn flag FALSE to the Tile currently on
    /// </summary>
    public void ResetOnCharactor() { ResetOnCharactor(tf.position); }

    /// <summary>
    /// Set IsCharactorOn flag FALSE to the Tile specified by Vector3 position
    /// </summary>
    /// <param name="pos">Vector3 Tile position</param>
    public void ResetOnCharactor(Vector3 pos) { ResetOnCharactor(map.MapPos(pos)); }

    /// <summary>
    /// Set IsCharactorOn flag FALSE to the Tile specified by Vector3 position
    /// </summary>
    /// <param name="pos">Pos unit Tile position</param>
    public void ResetOnCharactor(Pos pos)
    {
        map.GetTile(pos).IsCharactorOn = false;
    }
}