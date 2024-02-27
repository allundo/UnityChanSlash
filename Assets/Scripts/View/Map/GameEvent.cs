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
    private PlaceEnemyGenerator placeEnemyGenerator;

    public DropStartEvent(PlayerInput input, PlaceEnemyGenerator placeEnemyGenerator) : base(input)
    {
        map = input.playerMap;
        dropFloor = new PlayerDropFloor(input.playerTarget, 220f);
        this.placeEnemyGenerator = placeEnemyGenerator;
    }

    protected override IObservable<Unit> EventFunc()
    {
        input.Interrupt(dropFloor);
        placeEnemyGenerator.DisableAllEnemyGenerators();
        return RestartEventFunc();
    }

    protected override IObservable<Unit> RestartEventFunc()
    {
        return input.ObserveCompleteMessage(
            new MessageData
            (
                new MessageSource("いきなりなんという不条理・・・", FaceID.DISATTRACT),
                new MessageSource("ちょっと扱いが雑じゃない！？", FaceID.ANGRY)
            ),
            false
        ).ContinueWith(_ =>
        {
            float interval = 0.5f;

            if (map.RightTile is ExitDoor)
            {
                input.EnqueueTurnR();
                interval += 0.3f;
            }
            else if (map.LeftTile is ExitDoor)
            {
                input.EnqueueTurnL();
                interval += 0.3f;
            }
            else // The backward tile is ExitDoor
            {
                input.EnqueueTurnL();
                input.EnqueueTurnL();
                interval += 0.6f;
            }

            return Observable.Timer(TimeSpan.FromSeconds(interval))
                .ContinueWith(_ =>
                input.ObserveCompleteMessage(
                    new MessageData
                    (
                        new MessageSource("なんか使う看板まちがってる気がするけど", FaceID.DEFAULT),
                        new MessageSource("どうみてもこれが出口だね", FaceID.NOTICE),
                        new MessageSource("・・・うーん", FaceID.DISATTRACT),
                        new MessageSource("鍵が掛かってますねぇ！", FaceID.DISATTRACT)
                    )
                ));
        })
        .ContinueWith(cmd =>
        {
            placeEnemyGenerator.EnableAllEnemyGenerators();
            return Observable.Return(Unit.Default);
        });
    }
}

public class RestartEvent : GameEvent
{
    public RestartEvent(PlayerInput input) : base(input) { }

    protected override IObservable<Unit> EventFunc()
    {
        var message =
            new MessageData
            (
                new MessageSource("・・・という夢だったのさ", FaceID.SMILE),
                new MessageSource("絶対に抜け出してやるんだから！", FaceID.ANGRY)
            )
        ;

        return input.ObserveCompleteMessage(message);
    }
}