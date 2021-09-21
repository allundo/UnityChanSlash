using System.Collections.Generic;

public interface ITile
{
    bool IsEnterable(IDirection dir = null);
    bool IsLeapable { get; }
    bool IsViewOpen { get; }
    bool IsCharactorOn { get; }
    MobStatus OnCharacter { get; set; }
}

public class Ground : ITile
{
    public bool IsEnterable(IDirection dir = null) => !IsCharactorOn;
    public bool IsLeapable => true;
    public bool IsViewOpen => true;
    public bool IsCharactorOn => status != null;
    public MobStatus OnCharacter { get { return status; } set { status = value; } }
    public MobStatus status = null;
}

public class Wall : ITile
{
    public bool IsEnterable(IDirection dir = null) => false;
    public bool IsLeapable => false;
    public bool IsViewOpen => false;
    public bool IsCharactorOn => false;
    public MobStatus OnCharacter { get { return null; } set { } }
}

public class Door : ITile
{
    public bool IsEnterable(IDirection dir = null) => state.IsOpen && !IsCharactorOn;
    public bool IsLeapable => false;
    public bool IsViewOpen => state.IsOpen;
    public bool IsCharactorOn => state.IsCharactorOn;
    public MobStatus OnCharacter { get { return state.onCharacter; } set { state.onCharacter = value; } }

    public DoorState state { protected get; set; }
    public bool IsOpen => state.IsOpen;
    public bool IsControllable => state.IsControllable;
}

public class Stair : ITile
{
    public bool IsEnterable(IDirection dir = null) => enterDir.IsInverse(dir);
    public bool IsLeapable => false;
    public bool IsViewOpen => true;
    public bool IsCharactorOn => false;
    public MobStatus OnCharacter { get { return null; } set { } }

    public IDirection enterDir { protected get; set; }
    public bool isUpStair;
}
