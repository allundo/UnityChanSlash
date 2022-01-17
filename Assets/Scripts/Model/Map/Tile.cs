using System.Collections.Generic;

public interface ITile
{
    bool IsEnterable(IDirection dir = null);
    bool IsLeapable { get; }
    bool IsViewOpen { get; }
    bool IsObjectOn { get; set; }
    bool IsEnemyOn { get; }
    bool IsItemOn { get; }
    EnemyStatus OnEnemy { get; set; }
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
    public bool IsEnterable(IDirection dir = null) => !IsObjectOn;
    public bool IsLeapable => true;
    public bool IsViewOpen => true;
    public bool IsObjectOn { get; set; } = false;
    public bool IsEnemyOn => OnEnemy != null;
    public EnemyStatus OnEnemy { get; set; } = null;
}

public class Wall : Tile, ITile
{
    public bool IsEnterable(IDirection dir = null) => false;
    public bool IsLeapable => false;
    public bool IsViewOpen => false;
    public bool IsObjectOn { get { return false; } set { } }
    public bool IsEnemyOn => false;
    public EnemyStatus OnEnemy { get { return null; } set { } }
    public override bool PutItem(Item item) => false;
    public override Item PickItem() => null;

}

public class Door : Tile, ITile
{
    public bool IsEnterable(IDirection dir = null) => state.IsOpen && !IsObjectOn;
    public bool IsLeapable => false;
    public bool IsViewOpen => state.IsOpen;
    public bool IsObjectOn { get { return state.isObjectOn; } set { state.isObjectOn = value; } }
    public bool IsEnemyOn => OnEnemy != null;
    public EnemyStatus OnEnemy { get; set; } = null;

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
    public bool IsObjectOn { get { return false; } set { } }
    public bool IsEnemyOn => false;
    public EnemyStatus OnEnemy { get { return null; } set { } }

    public override bool PutItem(Item item) => false;
    public override Item PickItem() => null;

    public IDirection enterDir { protected get; set; }
    public bool isDownStairs;
}
