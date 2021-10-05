using UnityEngine;

/// <summary>
/// Map の Tile の状態を更新するメソッドを公開 <br>
/// Map の状態とキャラクターの位置・向きに応じた情報を提供するメソッドを公開
/// </summary>
public class MapUtil : MonoBehaviour
{
    protected WorldMap map;
    private Transform tf;
    public IDirection dir { get; protected set; }
    protected Pos onTilePos;
    protected MobStatus status;
    public static readonly float TILE_UNIT = 2.5f;

    private static readonly Pos defaultPos = new Pos(20, 20);
    private static readonly IDirection defaultDir = new North();

    protected virtual void Awake()
    {
        this.map = GameManager.Instance.worldMap;
        this.tf = transform;
        status = GetComponent<MobStatus>();
    }

    public virtual void SetPosition(bool isOnCharacter = true)
        => SetPosition(defaultPos, defaultDir, isOnCharacter);

    public void SetPosition(Pos pos, IDirection dir = null, bool isOnCharacter = true)
        => SetPosition(map.WorldPos(pos), dir, isOnCharacter);

    public void SetPosition(Vector3 pos, IDirection dir = null, bool isOnCharacter = true)
    {
        this.tf.position = pos;

        this.dir = dir ?? MapUtil.defaultDir;
        tf.LookAt(this.tf.position + this.dir.LookAt);

        if (isOnCharacter) SetOnCharacter();
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

    public Vector3 GetForwardVector(int distance = 1) => dir.LookAt * TILE_UNIT * distance;
    public Vector3 GetBackwardVector(int distance = 1) => -dir.LookAt * TILE_UNIT * distance;
    public Vector3 GetRightVector(int distance = 1) => Quaternion.Euler(0, 90, 0) * dir.LookAt * TILE_UNIT * distance;
    public Vector3 GetLeftVector(int distance = 1) => Quaternion.Euler(0, -90, 0) * dir.LookAt * TILE_UNIT * distance;

    public ITile ForwardTile => map.GetTile(GetForward);
    public ITile BackwardTile => map.GetTile(GetBackward);
    public ITile RightTile => map.GetTile(GetRight);
    public ITile LeftTile => map.GetTile(GetLeft);
    public ITile JumpTile => map.GetTile(GetJump);

    public bool IsCharactorOn(Pos destPos) => map.GetTile(destPos).IsCharacterOn;
    public bool IsMovable(Pos destPos, IDirection dir = null) => map.GetTile(destPos).IsEnterable(dir);
    public bool IsLeapable(Pos destPos) => map.GetTile(destPos).IsLeapable;

    public Vector3 CurrentVec3Pos => tf.position;
    public Pos CurrentPos => onTilePos;
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
    /// Set IsCharactorOn flag TRUE to the Tile currently on
    /// </summary>
    /// <returns>destPos</returns>
    public Pos SetOnCharacter() => SetOnCharacter(tf.position);

    /// <summary>
    /// Set IsCharactorOn flag TRUE to the Tile specified by Vector3 position
    /// </summary>
    /// <param name="destPos">Vector3 position of destination Tile</param>
    /// <returns>destPos</returns>
    public Pos SetOnCharacter(Vector3 destPos) => SetOnCharacter(map.MapPos(destPos));

    /// <summary>
    /// Set IsCharactorOn flag TRUE to the Tile specified by Pos unit
    /// </summary>
    /// <param name="destPos">Tile map position of destination</param>
    /// <returns>destPos</returns>
    public Pos SetOnCharacter(Pos destPos)
    {
        map.GetTile(destPos).OnCharacter = status;
        onTilePos = destPos;
        return destPos;
    }

    /// <summary>
    /// Set IsCharactorOn flag FALSE to the Tile currently on
    /// </summary>
    public void ResetOnCharacter() { ResetOnCharacter(tf.position); }

    /// <summary>
    /// Set IsCharactorOn flag FALSE to the Tile specified by Vector3 position
    /// </summary>
    /// <param name="pos">Vector3 Tile position</param>
    public void ResetOnCharacter(Vector3 pos) { ResetOnCharacter(map.MapPos(pos)); }

    /// <summary>
    /// Set IsCharactorOn flag FALSE to the Tile specified by Vector3 position
    /// </summary>
    /// <param name="pos">Pos unit Tile position</param>
    public void ResetOnCharacter(Pos pos)
    {
        map.GetTile(pos).OnCharacter = null;
    }

    /// <summary>
    /// Apply ResetOnCharacter at current Tile and SetOnCharacter to the dest Pos.
    /// </summary>
    public Pos MoveOnCharacter(Pos destPos)
    {
        ResetOnCharacter(onTilePos);
        return SetOnCharacter(destPos);
    }

}