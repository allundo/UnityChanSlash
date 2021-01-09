using System;
using UnityEngine;

public interface Tile
{
    bool IsEnterable();
    bool IsLeapable();
}

public class Ground : Tile
{
    public bool IsCharactorOn { protected get; set; } = false;
    public bool IsEnterable() => !IsCharactorOn;
    public bool IsLeapable() => true;
}

public class Wall : Tile
{
    public bool IsEnterable() => false;
    public bool IsLeapable() => false;
}

public class Door : Tile
{
    public DoorControl dc { protected get; set; }
    public bool IsEnterable() => dc.IsOpen;
    public bool IsLeapable() => false;
}