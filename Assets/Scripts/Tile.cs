using System;
using UnityEngine;

public interface Tile
{
    bool IsEnterable();
    bool IsLeapable();
    bool IsViewOpen();
}

public class Ground : Tile
{
    public bool IsCharactorOn { protected get; set; } = false;
    public bool IsEnterable() => !IsCharactorOn;
    public bool IsLeapable() => true;
    public bool IsViewOpen() => true;
}

public class Wall : Tile
{
    public bool IsEnterable() => false;
    public bool IsLeapable() => false;
    public bool IsViewOpen() => false;
}

public class Door : Tile
{
    public bool IsCharactorOn { protected get; set; } = false;
    public DoorControl dc { protected get; set; }
    public bool IsEnterable() => dc.IsOpen && !IsCharactorOn;
    public bool IsLeapable() => false;
    public bool IsViewOpen() => dc.IsOpen;
}