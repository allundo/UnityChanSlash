using UnityEngine;

public interface Tile
{
    bool IsEnterable(Direction dir = null);
    bool IsLeapable();
    bool IsViewOpen();
    bool IsCharactorOn { get; set; }
}

public class Ground : Tile
{
    public bool IsEnterable(Direction dir = null) => !IsCharactorOn;
    public bool IsLeapable() => true;
    public bool IsViewOpen() => true;
    public bool IsCharactorOn { get; set; } = false;
}

public class Wall : Tile
{
    public bool IsEnterable(Direction dir = null) => false;
    public bool IsLeapable() => false;
    public bool IsViewOpen() => false;
    public bool IsCharactorOn { get { return false; } set { } }
}

public class Door : Tile
{
    public DoorState state { protected get; set; }
    public bool IsEnterable(Direction dir = null) => state.IsOpen && !IsCharactorOn;
    public bool IsLeapable() => false;
    public bool IsViewOpen() => state.IsOpen;
    public bool IsCharactorOn
    {
        get
        {
            return state.IsCharactorOn;
        }
        set
        {
            state.IsCharactorOn = value;
        }
    }

    public Vector3 Position => state.transform.position;
    public bool IsOpen => state.IsOpen;
    public bool IsControllable => state.IsControllable;
}

public class Stair : Tile
{
    public Direction enterDir { protected get; set; }
    public bool isUpStair;
    public bool IsEnterable(Direction dir = null) => enterDir.IsInverse(dir);
    public bool IsLeapable() => false;
    public bool IsViewOpen() => true;
    public bool IsCharactorOn { get { return false; } set { } }
}
