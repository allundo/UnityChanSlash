using System.Collections.Generic;

public interface ITile
{
    bool IsEnterable(IDirection dir = null);
    bool IsLeapable { get; }
    bool IsViewOpen { get; }
    bool IsCharacterOn { get; }
    bool IsEnemyOn { get; }
    bool IsItemOn { get; }
    IEnemyStatus OnEnemy { get; set; }
    IEnemyStatus AboveEnemy { get; set; }
    IStatus OnCharacterDest { get; set; }
    bool PutItem(Item item);
    Item PickItem();
    ItemInfo TopItem { get; }
    IEnemyStatus GetEnemyStatus();
}

public interface IOpenable : ITile
{
    void Open();
    bool IsOpen { get; }
}

public interface IHandleTile : IOpenable
{
    void Handle();
    bool IsLocked { get; }
}

public class Tile
{
    protected Stack<Item> items = new Stack<Item>();
    public virtual bool IsItemOn => items.Count > 0;

    public virtual IEnemyStatus OnEnemy { get; set; } = null;
    public virtual IEnemyStatus AboveEnemy { get; set; } = null;
    public virtual IStatus OnCharacterDest { get; set; } = null;

    public virtual bool IsCharacterOn => OnCharacterDest != null;
    public virtual bool IsEnemyOn => OnEnemy != null || OnCharacterDest is IEnemyStatus || AboveEnemy != null;

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

    public virtual ItemInfo TopItem => items.Count > 0 ? items.Peek().itemInfo : null;

    public virtual IEnemyStatus GetEnemyStatus()
        => OnEnemy ?? (OnCharacterDest is IEnemyStatus ? OnCharacterDest as IEnemyStatus : null) ?? AboveEnemy;
}

public abstract class HandleTile : Tile
{
    public void Open() => Handle();
    public abstract void Handle();
}

public class Ground : Tile, ITile
{
    public bool IsEnterable(IDirection dir = null) => !IsCharacterOn;
    public bool IsLeapable => true;
    public bool IsViewOpen => true;
}

public class Wall : Tile, ITile
{
    public bool IsEnterable(IDirection dir = null) => false;
    public bool IsLeapable => false;
    public bool IsViewOpen => false;
    public override bool IsCharacterOn => false;
    public override bool IsEnemyOn => AboveEnemy != null;
    public override IEnemyStatus OnEnemy { get { return null; } set { } }
    public override IStatus OnCharacterDest { get { return null; } set { } }
    public override bool PutItem(Item item) => false;
    public override Item PickItem() => null;
    public override ItemInfo TopItem => null;

}

public class MessageWall : Wall
{
    public bool IsReadable(IDirection dir = null) => boardDir.IsInverse(dir);
    public MessageData[] Read => data;
    public IDirection boardDir { protected get; set; }
    public MessageData[] data { protected get; set; }
}

public class Door : HandleTile, IHandleTile
{
    public bool IsEnterable(IDirection dir = null) => state.IsOpen && !IsCharacterOn;
    public bool IsLeapable => false;
    public virtual bool IsViewOpen => state.IsOpen;
    public override bool IsCharacterOn => state.IsCharacterOn;
    public override IStatus OnCharacterDest { get { return state.onCharacterDest; } set { state.onCharacterDest = value; } }

    public DoorState state { protected get; set; }
    public override void Handle() => state.TransitToNextState();
    public bool IsOpen => state.IsOpen;
    public bool IsLocked => state.IsLocked;
    public bool IsControllable => state.IsControllable;
    public bool UnLock(ItemType type) => state.Unlock(type);

    public override bool PutItem(Item item) => IsOpen ? base.PutItem(item) : false;
    public override Item PickItem() => IsOpen ? base.PickItem() : null;
    public override ItemInfo TopItem => IsOpen ? base.TopItem : null;
}

public class ExitDoor : Door
{
    public override bool IsViewOpen => false;
}

public class Box : HandleTile, IHandleTile
{
    public bool IsEnterable(IDirection dir = null) => false;
    public bool IsLeapable => true;
    public virtual bool IsViewOpen => true;
    public override bool IsCharacterOn => false;

    public override bool IsEnemyOn => AboveEnemy != null;
    public override IEnemyStatus OnEnemy { get { return null; } set { } }
    public override IStatus OnCharacterDest { get { return null; } set { } }

    public BoxState state { protected get; set; }
    public override void Handle() => state.TransitToNextState();
    public bool IsOpen => state.IsOpen;
    public bool IsLocked => state.IsLocked;
    public bool IsControllable => state.IsControllable;

    public override bool PutItem(Item item)
    {
        bool isPut = base.PutItem(item);
        if (isPut) item.SetDisplay(false);
        return isPut;
    }

    public override Item PickItem()
    {
        Item item = base.PickItem();
        item?.SetDisplay(true);
        return item;
    }

    public override ItemInfo TopItem => null;
}
public class Pit : Tile, IOpenable
{
    public bool IsEnterable(IDirection dir = null) => dir != null;
    public bool IsLeapable => true;
    public virtual bool IsViewOpen => true;
    public override bool IsCharacterOn => false;

    public override bool IsEnemyOn => AboveEnemy != null;
    public override IEnemyStatus OnEnemy { get { return null; } set { } }
    public override IStatus OnCharacterDest { get { return null; } set { } }

    public PitState state { protected get; set; }
    public void Open() => state.Drop(false);
    public bool IsOpen => state.isDropped;
    public float Damage => state.damage;

    public override bool PutItem(Item item) => false;
    public override Item PickItem() => null;
    public override ItemInfo TopItem => null;
}

public class Stairs : Tile, ITile
{
    public bool IsEnterable(IDirection dir = null) => enterDir.IsInverse(dir);
    public bool IsLeapable => false;
    public bool IsViewOpen => true;
    public override bool IsCharacterOn => false;
    public override bool IsEnemyOn => AboveEnemy != null;
    public override IEnemyStatus OnEnemy { get { return null; } set { } }
    public override IStatus OnCharacterDest { get { return null; } set { } }

    public override bool PutItem(Item item) => false;
    public override Item PickItem() => null;

    public IDirection enterDir { protected get; set; }
}
