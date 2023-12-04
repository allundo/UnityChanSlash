using System;
using UniRx;

public abstract class GameEvent
{
    protected abstract IObservable<Unit> EventFunc();
    protected virtual IObservable<Unit> RestartEventFunc() => EventFunc();
    public virtual IObservable<Unit> Invoke(bool isRestart = false) => isRestart ? RestartEventFunc() : EventFunc();
    protected PlayerInput input;

    public GameEvent(PlayerInput input)
    {
        this.input = input;
    }
}

public class DropStartEvent : GameEvent
{
    private ICommand dropFloor;
    private IPlayerMapUtil map;
    public DropStartEvent(PlayerInput input) : base(input)
    {
        map = input.playerMap;
        dropFloor = new PlayerDropFloor(input.playerTarget, 220f);
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
                new MessageSource("いきなりなんという不条理・・・", FaceID.DISATTRACT),
                new MessageSource("ちょっと扱いが雑じゃない！？", FaceID.ANGRY)
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
                new MessageSource("なんか使う看板まちがってる気がするけど", FaceID.DEFAULT),
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
    public RestartEvent(PlayerInput input) : base(input) { }

    protected override IObservable<Unit> EventFunc()
    {
        ICommand message = input.InterruptMessage(
            new MessageData
            (
                new MessageSource("・・・という夢だったのさ", FaceID.SMILE),
                new MessageSource("絶対に抜け出してやるんだから！", FaceID.ANGRY)
            )
        );

        return input.ObserveComplete(message).Select(_ => Unit.Default);
    }
}