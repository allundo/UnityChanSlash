using System.Collections.Generic;
using UnityEngine;

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
    bool HasItem(ItemType type);
    IEnemyStatus GetEnemyStatus();
    Stack<Item> items { get; set; }
}

public interface IOpenable : ITile
{
    IOpenState state { get; }
    void Open();
    bool IsOpen { get; }
}

public interface IHandleTile : IOpenable
{
    void Handle();
    bool IsLocked { get; }
}

public interface IReadable : ITile
{
    void Read();
    bool IsRead { get; }
}

public class Tile
{
    public Stack<Item> items { get; set; } = new Stack<Item>();
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

    public bool HasItem(ItemType type) => items.Contains(item => item.itemInfo.type == type);

    public virtual IEnemyStatus GetEnemyStatus()
        => OnEnemy ?? OnCharacterDest as IEnemyStatus ?? AboveEnemy;
}

public class OpenTile : Tile
{
    public IOpenState state { get; protected set; }
    public OpenTile(IOpenState state) => this.state = state;

    public virtual void Open() => state.Open();
    public virtual bool IsOpen => state.IsOpen;
}

public abstract class HandleTile : OpenTile
{
    public HandleTile(IHandleState state) : base(state) { }
    public override void Open() => Handle();
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
    public virtual bool IsLeapable => false;
    public virtual bool IsViewOpen => false;

    public override IStatus OnCharacterDest { get { return null; } set { } }
    public override bool IsCharacterOn => false;

    public override bool PutItem(Item item) => false;
    public override Item PickItem() => null;
    public override ItemInfo TopItem => null;
}

public class MessageWall : Wall, IReadable
{
    public bool IsReadable(IDirection dir = null) => boardDir.IsInverse(dir);
    public void Read() => data.Read();
    public bool IsRead => data.isRead;
    public IDirection boardDir { protected get; set; }
    public MessageData data;
}

public class Door : HandleTile, IHandleTile
{
    protected DoorState doorState;
    public Door(ItemType keyItem = ItemType.Null) : this(new DoorState(keyItem)) { }
    protected Door(DoorState state) : base(state) => this.doorState = state as DoorState;

    public override void Open() => doorState.Open();
    public override void Handle() => doorState.TransitToNextState();

    public virtual bool IsEnterable(IDirection dir = null) => doorState.IsOpen && !IsCharacterOn;
    public bool IsLeapable => false;
    public virtual bool IsViewOpen => doorState.IsOpen;
    public override bool IsCharacterOn => doorState.IsCharacterOn;
    public override IStatus OnCharacterDest
    {
        get { return IsOpen ? doorState.onCharacterDest : null; }
        set { if (IsOpen) doorState.onCharacterDest = value; }
    }

    public bool IsLocked => doorState.IsLocked;
    public bool IsControllable => doorState.IsControllable;
    public bool Unlock(ItemType type) => doorState.Unlock(type);

    public void Break() => doorState.Break();
    public bool IsBroken => doorState.isBroken;

    public override bool PutItem(Item item) => IsOpen ? base.PutItem(item) : false;
    public override Item PickItem() => IsOpen ? base.PickItem() : null;
    public override ItemInfo TopItem => IsOpen ? base.TopItem : null;
}

public class OpenDoor : Door, IEventTile
{
    public IEventHandleState eventState => state as EventFixedOpenDoorState;
    public OpenDoor(ItemType keyItem = ItemType.Null) : base(new EventFixedOpenDoorState(keyItem)) { }
}

public class SealableDoor : Door, IEventTile
{
    public IEventHandleState eventState => state as EventSealedCloseDoorState;
    public SealableDoor(ItemType keyItem = ItemType.Null) : base(new EventSealedCloseDoorState(keyItem)) { }
    public override bool IsEnterable(IDirection dir = null) => !eventState.isEventOn && doorState.IsOpen && !IsCharacterOn;
}

public class ExitDoor : Door
{
    public ExitDoor() : base(ItemType.KeyBlade) { }
    public override bool IsViewOpen => false;
    public void Unlock() => doorState.Unlock();
}

public class Box : HandleTile, IHandleTile
{
    protected BoxState boxState;
    public Box() : base(new BoxState()) => this.boxState = state as BoxState;
    public bool IsEnterable(IDirection dir = null) => false;
    public bool IsLeapable => true;
    public virtual bool IsViewOpen => true;

    public bool IsLocked => boxState.IsLocked;
    public bool IsControllable => boxState.IsControllable;
    public override void Handle() => boxState.TransitToNextState();

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

    public override ItemInfo TopItem => null; // Top item is not visible in the closed Box.
}

public class Pit : OpenTile, IOpenable
{
    protected PitState pitState;
    public Pit(int floor) : base(new PitState(floor)) => this.pitState = state as PitState;
    public bool IsEnterable(IDirection dir = null) => dir != null;
    public bool IsLeapable => true;
    public virtual bool IsViewOpen => true;

    public void Drop() => pitState.Drop(true);
    public float Damage => pitState.damage;

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

public class EXStructure : Wall
{
    public virtual ActiveMessageData inspectMsg => msgData;
    protected ActiveMessageData msgData;
    public EXStructure(ActiveMessageData data) => msgData = data;

    public override bool IsViewOpen => true;
}

public class Furniture : EXStructure
{
    public Furniture(ActiveMessageData data) : base(data) { }
    public override bool IsLeapable => true;
}

public interface IEventTile : ITile
{
    IEventHandleState eventState { get; }
}

public class Fountain : EXStructure, IEventTile
{
    protected ActiveMessageData inspectCurse;
    protected ActiveMessageData drink;
    protected ActiveMessageData drinkCurse;

    public FountainState state { get; protected set; }
    public IEventHandleState eventState => state;

    public bool isEventOn => state.isEventOn;

    public Fountain() : base(new ActiveMessageData("魔力を帯びた水が流れ出ている。", SDFaceID.SURPRISE, SDEmotionID.SURPRISE))
    {
        inspectCurse = new ActiveMessageData("硫黄臭い熱湯が吹き出している！", SDFaceID.SURPRISE, SDEmotionID.EXQUESTION);
        drink = new ActiveMessageData("おいしい水！", SDFaceID.SMILE, SDEmotionID.WAIWAI);
        drinkCurse = new ActiveMessageData("アッツゥイ！！", SDFaceID.SAD2, SDEmotionID.IRRITATE);
        state = new FountainState();
    }

    public override ActiveMessageData inspectMsg
        => isEventOn ? inspectCurse : msgData;

    public void GetAction(PlayerCommandTarget target)
    {
        (target.anim as PlayerAnimator).getItem.Fire();

        if (isEventOn)
        {
            (target.react as PlayerReactor).DrinkPoison(0.1f, AttackAttr.Dark);
            ActiveMessageController.Instance.InputMessageData(drinkCurse);
        }
        else
        {
            (target.react as PlayerReactor).DrinkWater(1f);
            ActiveMessageController.Instance.InputMessageData(drink);
        }
    }
}
