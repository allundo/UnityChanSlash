using UnityEngine;

public interface Tile
{
    bool IsEnterable(Direction dir = null);
    bool IsLeapable { get; }
    bool IsViewOpen { get; }
    bool IsCharactorOn { get; }
    MobStatus OnCharacter { get; set; }
}

public class Ground : Tile
{
    public bool IsEnterable(Direction dir = null) => !IsCharactorOn;
    public bool IsLeapable => true;
    public bool IsViewOpen => true;
    public bool IsCharactorOn => status != null;
    public MobStatus OnCharacter { get { return status; } set { status = value; } }
    public MobStatus status = null;
}

public class Wall : Tile
{
    public bool IsEnterable(Direction dir = null) => false;
    public bool IsLeapable => false;
    public bool IsViewOpen => false;
    public bool IsCharactorOn => false;
    public MobStatus OnCharacter { get { return null; } set { } }
}

public class Door : Tile
{
    public bool IsEnterable(Direction dir = null) => state.IsOpen && !IsCharactorOn;
    public bool IsLeapable => false;
    public bool IsViewOpen => state.IsOpen;
    public bool IsCharactorOn => state.IsCharactorOn;
    public MobStatus OnCharacter { get { return state.onCharacter; } set { state.onCharacter = value; } }

    public DoorState state { protected get; set; }
    public bool IsOpen => state.IsOpen;
    public bool IsControllable => state.IsControllable;
}

public class Stair : Tile
{
    public bool IsEnterable(Direction dir = null) => enterDir.IsInverse(dir);
    public bool IsLeapable => false;
    public bool IsViewOpen => true;
    public bool IsCharactorOn => false;
    public MobStatus OnCharacter { get { return null; } set { } }

    public Direction enterDir { protected get; set; }
    public bool isUpStair;
}
