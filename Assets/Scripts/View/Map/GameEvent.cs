using System;
using UniRx;

public abstract class GameEvent
{
    protected abstract IObservable<Unit> EventFunc();
    protected virtual IObservable<Unit> RestartEventFunc() => EventFunc();
    public virtual IObservable<Unit> Invoke(bool isRestart = false) => isRestart ? RestartEventFunc() : EventFunc();
    protected PlayerCommandTarget target;
    protected PlayerInput input;
    protected IPlayerMapUtil map;

    public GameEvent(PlayerCommandTarget target)
    {
        this.target = target;
        input = target.input as PlayerInput;
        map = target.map as PlayerMapUtil;
    }
}

public class DropStartEvent : GameEvent
{
    private ICommand dropFloor;
    public DropStartEvent(PlayerCommandTarget target) : base(target)
    {
        dropFloor = new PlayerDropFloor(target, 220f);
    }

    protected override IObservable<Unit> EventFunc()
    {
        input.Interrupt(dropFloor);
        return RestartEventFunc();
    }

    protected override IObservable<Unit> RestartEventFunc()
    {
        input.EnqueueMessage(
            new MessageData
            (
                new MessageSource("いきなりなんなのさ・・・", FaceID.DISATTRACT),
                new MessageSource("久々の出番なのに、扱いが雑じゃない！？", FaceID.ANGRY)
            ),
            false
        );

        if (map.RightTile is ExitDoor)
        {
            input.EnqueueTurnR();
        }
        else if (map.LeftTile is ExitDoor)
        {
            input.EnqueueTurnL();
        }
        else // The backward tile is ExitDoor
        {
            input.EnqueueTurnL();
            input.EnqueueTurnL();
        }

        ICommand message = input.EnqueueMessage(
            new MessageData
            (
                new MessageSource("なんか使う標示まちがってる気がするけど", FaceID.DEFAULT),
                new MessageSource("どうみてもこれが出口だね", FaceID.NOTICE),
                new MessageSource("・・・うーん", FaceID.DISATTRACT),
                new MessageSource("鍵が掛かってますねぇ！", FaceID.DISATTRACT)
            )
        );

        return input.ObserveComplete(message).Select(_ => Unit.Default);
    }
}

public class RestartEvent : GameEvent
{
    public RestartEvent(PlayerCommandTarget target) : base(target) { }

    protected override IObservable<Unit> EventFunc()
    {
        ICommand message = input.InterruptMessage(
            new MessageData
            (
                new MessageSource("[仮] ・・・という夢だったのさ", FaceID.SMILE),
                new MessageSource("[仮] なんも解決してないんだけどねっ！", FaceID.ANGRY)
            )
        );

        return input.ObserveComplete(message).Select(_ => Unit.Default);
    }
}