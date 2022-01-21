using UnityEngine;

public interface IMapUtil
{
    IDirection dir { get; }

    void OnActive();
    void ResetTile();
    Vector3 WorldPos(Pos pos);

    void TurnLeft();
    void TurnRight();

    Vector3 GetForwardVector(int distance = 1);
    Vector3 GetBackwardVector(int distance = 1);
    Vector3 GetRightVector(int distance = 1);
    Vector3 GetLeftVector(int distance = 1);

    ITile GetTile(Pos destPos);
    ITile ForwardTile { get; }
    ITile BackwardTile { get; }
    ITile RightTile { get; }
    ITile LeftTile { get; }
    ITile JumpTile { get; }

    bool IsObjectOn(Pos destPos);
    bool IsMovable(Pos destPos, IDirection dir = null);
    bool IsLeapable(Pos destPos);

    Vector3 CurrentVec3Pos { get; }
    Pos CurrentPos { get; }
    Vector3 DestVec { get; }

    Pos GetForward { get; }
    Pos GetLeft { get; }
    Pos GetRight { get; }
    Pos GetBackward { get; }
    Pos GetJump { get; }

    bool IsForwardMovable { get; }
    bool IsForwardLeapable { get; }
    bool IsBackwardMovable { get; }
    bool IsLeftMovable { get; }
    bool IsRightMovable { get; }
    bool IsJumpable { get; }

    bool IsPlayerFound();
    bool IsPlayerFound(Pos pos);

    bool IsPlayerForward { get; }
    bool IsPlayerLeft { get; }
    bool IsPlayerRight { get; }
    bool IsPlayerBackward { get; }

    /// <summary>
    /// Set current on tile and IsObjectOn flag to the Tile currently on
    /// </summary>
    /// <returns>Current map Pos</returns>
    Pos SetObjectOn();

    /// <summary>
    /// Set current on tile and IsObjectOn flag to the Tile specified by Vector3 position
    /// </summary>
    /// <param name="destPos">Vector3 position of destination Tile</param>
    /// <returns>destPos</returns>
    Pos SetObjectOn(Vector3 destPos);

    /// <summary>
    /// Set current on tile and IsObjectOn flag to the Tile specified by Pos unit
    /// </summary>
    /// <param name="destPos">Tile map position of destination</param>
    /// <returns>destPos</returns>
    Pos SetObjectOn(Pos destPos);

    /// <summary>
    /// Set IsObjectOn flag FALSE to the Tile currently on
    /// </summary>
    void RemoveObjectOn();

    /// <summary>
    /// Set IsObjectOn flag FALSE to the Tile specified by Vector3 position
    /// </summary>
    /// <param name="pos">Vector3 Tile position</param>
    void RemoveObjectOn(Vector3 pos);

    /// <summary>
    /// Set IsObjectOn flag FALSE to the Tile specified by Vector3 position
    /// </summary>
    /// <param name="pos">Pos unit Tile position</param>
    void RemoveObjectOn(Pos pos);

    /// <summary>
    /// Apply RemoveObjectOn at current Tile and SetObjectOn to the dest Pos.
    /// </summary>
    Pos MoveObjectOn(Vector3 destPos);

    /// <summary>
    /// Apply RemoveObjectOn at current Tile and SetObjectOn to the dest Pos.
    /// </summary>
    Pos MoveObjectOn(Pos destPos);
}