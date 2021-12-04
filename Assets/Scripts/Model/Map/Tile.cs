using System.Collections.Generic;

public interface ITile
{
    bool IsEnterable(IDirection dir = null);
    bool IsLeapable { get; }
    bool IsViewOpen { get; }
    bool IsCharacterOn { get; }
    bool IsItemOn { get; }
    MobStatus OnCharacter { get; set; }
    bool PutItem(Item item);
    Item PickItem();
}

public class Tile
{
    protected Stack<Item> items = new Stack<Item>();
    public virtual bool IsItemOn => items.Count > 0;

    public virtual bool PutItem(Item item)
    {
        if (item == null) return false;

        items.Push(item);
        return true;
    }

    public virtual Item PickItem()
    {
        if (items.Count == 0) return null;

        var item = items.Pop();
        item.Inactivate();

        return item;
    }
}

public class Ground : Tile, ITile
{
    public bool IsEnterable(IDirection dir = null) => !IsCharacterOn;
    public bool IsLeapable => true;
    public bool IsViewOpen => true;
    public bool IsCharacterOn => status != null && status.IsOnGround;
    public MobStatus OnCharacter { get { return status; } set { status = value; } }
    public MobStatus status = null;
}

public class Wall : Tile, ITile
{
    public bool IsEnterable(IDirection dir = null) => false;
    public bool IsLeapable => false;
    public bool IsViewOpen => false;
    public bool IsCharacterOn => false;
    public MobStatus OnCharacter { get { return null; } set { } }
    public override bool PutItem(Item item) => false;
    public override Item PickItem() => null;
}

public class Door : Tile, ITile
{
    public bool IsEnterable(IDirection dir = null) => state.IsOpen && !IsCharacterOn;
    public bool IsLeapable => false;
    public bool IsViewOpen => state.IsOpen;
    public bool IsCharacterOn => state.onCharacter != null && state.onCharacter.IsOnGround;
    public MobStatus OnCharacter { get { return state.onCharacter; } set { state.onCharacter = value; } }

    public DoorState state { protected get; set; }
    public bool IsOpen => state.IsOpen;
    public bool IsControllable => state.IsControllable;

    public override bool PutItem(Item item) => IsOpen ? base.PutItem(item) : false;
    public override Item PickItem() => IsOpen ? base.PickItem() : null;
}

public class Stair : Tile, ITile
{
    public bool IsEnterable(IDirection dir = null) => enterDir.IsInverse(dir);
    public bool IsLeapable => false;
    public bool IsViewOpen => true;
    public bool IsCharacterOn => false;
    public MobStatus OnCharacter { get { return null; } set { } }

    public override bool PutItem(Item item) => false;
    public override Item PickItem() => null;

    public IDirection enterDir { protected get; set; }
    public bool isUpStair;
}
