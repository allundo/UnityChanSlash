using System.Collections.Generic;

public interface ITile
{
    bool IsEnterable(IDirection dir = null);
    bool IsLeapable { get; }
    bool IsViewOpen { get; }
    bool IsCharacterOn { get; }
    bool IsEnemyOn { get; }
    bool IsItemOn { get; }
    EnemyStatus OnEnemy { get; set; }
    MobStatus OnCharacterDest { get; set; }
    bool PutItem(Item item);
    Item PickItem();
    EnemyStatus GetEnemyStatus();
}

public class Tile
{
    protected Stack<Item> items = new Stack<Item>();
    public virtual bool IsItemOn => items.Count > 0;

    public virtual EnemyStatus OnEnemy { get; set; } = null;
    public virtual MobStatus OnCharacterDest { get; set; } = null;

    public virtual bool IsCharacterOn => OnCharacterDest != null;
    public virtual bool IsEnemyOn => OnEnemy != null || OnCharacterDest is EnemyStatus;

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

    public virtual EnemyStatus GetEnemyStatus()
        => OnEnemy ?? (OnCharacterDest is EnemyStatus ? OnCharacterDest as EnemyStatus : null);
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
    public override bool IsEnemyOn => false;
    public override EnemyStatus OnEnemy { get { return null; } set { } }
    public override MobStatus OnCharacterDest { get { return null; } set { } }
    public override bool PutItem(Item item) => false;
    public override Item PickItem() => null;

}

public class Door : Tile, ITile
{
    public bool IsEnterable(IDirection dir = null) => state.IsOpen && !IsCharacterOn;
    public bool IsLeapable => false;
    public bool IsViewOpen => state.IsOpen;
    public override bool IsCharacterOn => state.IsCharacterOn;
    public override MobStatus OnCharacterDest { get { return state.onCharacterDest; } set { state.onCharacterDest = value; } }

    public DoorState state { protected get; set; }
    public void Handle() => state.Handle();
    public bool IsOpen => state.IsOpen;
    public bool IsControllable => state.IsControllable;

    public override bool PutItem(Item item) => IsOpen ? base.PutItem(item) : false;
    public override Item PickItem() => IsOpen ? base.PickItem() : null;
}

public class Stairs : Tile, ITile
{
    public bool IsEnterable(IDirection dir = null) => enterDir.IsInverse(dir);
    public bool IsLeapable => false;
    public bool IsViewOpen => true;
    public override bool IsCharacterOn => false;
    public override bool IsEnemyOn => false;
    public override EnemyStatus OnEnemy { get { return null; } set { } }
    public override MobStatus OnCharacterDest { get { return null; } set { } }

    public override bool PutItem(Item item) => false;
    public override Item PickItem() => null;

    public IDirection enterDir { protected get; set; }
    public bool isDownStairs;
}
