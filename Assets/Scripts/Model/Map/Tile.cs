using System;
using UnityEngine;

public interface Tile
{
    bool IsEnterable();
    bool IsLeapable();
    bool IsViewOpen();
    bool IsCharactorOn { get; set; }
}

public class Ground : Tile
{
    public bool IsEnterable() => !IsCharactorOn;
    public bool IsLeapable() => true;
    public bool IsViewOpen() => true;
    public bool IsCharactorOn { get; set; } = false;
}

public class Wall : Tile
{
    public bool IsEnterable() => false;
    public bool IsLeapable() => false;
    public bool IsViewOpen() => false;
    public bool IsCharactorOn { get { return false; } set { } }
}

public class Door : Tile
{
    public DoorState state { protected get; set; }
    public bool IsEnterable() => state.IsOpen && !IsCharactorOn;
    public bool IsLeapable() => false;
    public bool IsViewOpen() => state.IsOpen;
    public bool IsCharactorOn { get; set; } = false;
}
