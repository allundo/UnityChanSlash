using System;
using UniRx;
using UnityEngine;
using DG.Tweening;

public abstract class PlayerDetectEvent : GameEvent
{
    public IObservable<Unit> EventEndDetector => eventEndSubject;
    protected ISubject<Unit> eventEndSubject = new Subject<Unit>();
    protected EventInvoker invoker;
    protected IDisposable disposable;
    public bool isOneShot { get; protected set; }

    protected abstract bool IsEventValid();

    public PlayerDetectEvent(PlayerCommandTarget target, bool isOneShot = true) : base(target)
    {
        this.isOneShot = isOneShot;
    }

    protected abstract Vector3 EventTilePosition(WorldMap map);

    public void Activate(EventInvoker invoker, WorldMap map)
    {
        this.invoker = invoker.OnSpawn(EventTilePosition(map));

        disposable = invoker.DetectPlayer
            .Where(_ => IsEventValid())
            .SelectMany(_ => Invoke())      // ContinueWith observes OnCompleted() AFTER OnNext() is sent.
            .Subscribe(_ =>
            {
                if (isOneShot)
                {
                    eventEndSubject.OnCompleted();
                    invoker.Inactivate();
                }
                else
                {
                    eventEndSubject.OnNext(Unit.Default);
                }
            })
            .AddTo(invoker);
    }

    public void Inactivate()
    {
        disposable?.Dispose();
        invoker.Inactivate();
    }
}

public class PlayerDetectFloor : PlayerDetectEvent
{
    protected MessageData[] eventMessages;

    public PlayerDetectFloor(PlayerCommandTarget target, MessageData[] eventMessages) : base(target)
    {
        this.eventMessages = eventMessages;
    }

    protected override bool IsEventValid() => true;
    protected override Vector3 EventTilePosition(WorldMap map) => map.WorldPos(map.StairsBottom.Key);

    protected override IObservable<Unit> EventFunc()
        => input
            .ObserveComplete(input.EnqueueMessage(eventMessages))
            .Select(_ => Unit.Default);
}

public class PlayerMessageFloor3 : PlayerDetectFloor
{
    public PlayerMessageFloor3(PlayerCommandTarget target) : base(target, new MessageData[]
    {
        new MessageData("そろそろ地下 3 階ってもんかしら？\n…なんかめっちゃ暑いんですけど。", FaceID.DEFAULT),
        new MessageData("体力が奪われてく感じがする…。\nこまめに休息とらないとしんどいね、こりゃ。", FaceID.DESPISE),
    })
    { }
}
public class PlayerMessageFloor4 : PlayerDetectFloor
{
    public PlayerMessageFloor4(PlayerCommandTarget target) : base(target, new MessageData[]
    {
        new MessageData("いよいよ地下 4 階！\n・・・って、寒ゥ！！！", FaceID.SURPRISE),
        new MessageData("フィンランド式サウナか！\n一体どうなってんの？！", FaceID.ANGRY),
        new MessageData("ここで寝たらアウトだね…。\n急いで突破しよ！", FaceID.ANGRY2),
    })
    { }
}

public class PlayerMessageFloor5 : PlayerDetectFloor
{
    public PlayerMessageFloor5(PlayerCommandTarget target) : base(target, new MessageData[]
    {
        new MessageData("地下 5 階とーちゃーく！\nうぇ〜寒かったぁ…。", FaceID.SMILE),
        new MessageData("…なんだかモノモノシイ雰囲気。\nこれが最後って感じかな？", FaceID.EYECLOSE),
        new MessageData("とっとと鍵奪って脱出してやりますかね！", FaceID.ANGRY2),
    })
    { }
}

public abstract class PlayerHasItemEvent : PlayerDetectEvent
{
    protected Pos pos;
    protected ItemType itemTypeToCheck;

    public PlayerHasItemEvent(PlayerCommandTarget target, Pos pos, ItemType itemTypeToCheck, bool isOneShot = true) : base(target, isOneShot)
    {
        this.itemTypeToCheck = itemTypeToCheck;
        this.pos = pos;
    }

    protected override Vector3 EventTilePosition(WorldMap map) => map.WorldPos(pos);

    protected override bool IsEventValid()
    {
        var playerHasItem = ItemInventory.Instance.hasItemType(itemTypeToCheck);
        var itemIsOnEventTile = target.map.OnTile.HasItem(itemTypeToCheck);
        var jumpLeapedEventTileWithItem = target.map.BackwardTile.HasItem(itemTypeToCheck);

        return playerHasItem || itemIsOnEventTile || jumpLeapedEventTileWithItem;
    }
}

public class SkeletonsGenerateEvent : PlayerHasItemEvent
{
    private DoorOpener prefabDoorOpener;
    public SkeletonsGenerateEvent(PlayerCommandTarget target, Pos pos) : base(target, pos, ItemType.TreasureKey)
    {
        prefabDoorOpener = Resources.Load<DoorOpener>("Prefabs/Map/DoorOpener");
    }

    protected override IObservable<Unit> EventFunc()
    {
        var spawn = SpawnHandler.Instance;
        var option = new EnemyStatus.ActivateOption();

        spawn.PlaceEnemy(EnemyType.SkeletonSoldier, new Pos(21, 1), Direction.west, option);
        spawn.PlaceEnemy(EnemyType.SkeletonSoldier, new Pos(21, 17), Direction.west, option);
        spawn.PlaceEnemy(EnemyType.SkeletonWizard, new Pos(24, 9), Direction.west, option);
        spawn.PlaceEnemy(EnemyType.SkeletonSoldier, new Pos(24, 11), Direction.west, option);
        spawn.PlaceEnemy(EnemyType.SkeletonWizard, new Pos(24, 13), Direction.west, option);
        spawn.PlaceEnemy(EnemyType.SkeletonSoldier, new Pos(24, 15), Direction.west, option);
        spawn.PlaceEnemy(EnemyType.SkeletonWizard, new Pos(24, 17), Direction.west, option);

        ActiveMessageController.Instance.InputMessageData("スケルトンがいっぱい！", SDFaceID.SURPRISE, SDEmotionID.EXSURPRISE);

        var map = GameManager.Instance.worldMap;
        var opener = Util.Instantiate(prefabDoorOpener, map.WorldPos(pos), Quaternion.identity);

        return opener.Shoot(map.WorldPos(new Pos(21, 17)))
            .ContinueWith(_ =>
            {
                target.hidePlateHandler.Redraw();
                UnityEngine.Object.Destroy(opener.gameObject, 0.1f);
                return Observable.NextFrame();
            });
    }
}

public class WitchGenerateEvent : PlayerHasItemEvent
{
    private LightManager lightManager;

    public WitchGenerateEvent(PlayerCommandTarget target, LightManager lightManager, Pos pos) : base(target, pos, ItemType.KeyBlade)
    {
        this.lightManager = lightManager;
    }

    protected override IObservable<Unit> EventFunc()
    {
        input.InputStop();
        input.SetInputVisible(false);

        SpawnHandler.Instance.EraseAllEnemies();

        ICommand message = input.EnqueueMessage(
            new MessageData[]
            {
                new MessageData("『ちょっと待ちなよ』", FaceID.NONE),
                new MessageData("・・・！", FaceID.SURPRISE),
            },
            false
        );

        return input.ObserveComplete(message)
            .ContinueWith(_ => WitchGenerateEventMain(Direction.south));
    }

    private IObservable<Unit> WitchGenerateEventMain(IDirection witchDir)
    {
        Sequence seq = DOTween.Sequence();
        Pos witchPos = map.GetForward;
        float interval = 0.5f;

        if (map.dir.IsInverse(witchDir))
        {
            seq
                .AppendCallback(() => input.EnqueueTurnL())
                .AppendCallback(() => input.EnqueueTurnL());

            witchPos = map.GetBackward;

            interval += 0.6f;
        }
        else if (map.dir.IsLeft(witchDir))
        {
            seq.AppendCallback(() => input.EnqueueTurnL());
            witchPos = map.GetLeft;
            interval += 0.3f;
        }
        else if (map.dir.IsRight(witchDir))
        {
            seq.AppendCallback(() => input.EnqueueTurnR());
            witchPos = map.GetRight;
            interval += 0.3f;
        }

        return seq
            .InsertCallback(0.5f, () => SpawnHandler.Instance.PlaceWitch(witchPos, witchDir.Backward, 300f))
            .Append(lightManager.DirectionalFadeOut(1.5f))
            .Join(lightManager.PointFadeOut(1.5f))
            .Join(lightManager.SpotFadeIn(map.WorldPos(witchPos) + new Vector3(0, 4f, 0), 1f, 30f, 1.0f))
            .AppendInterval(interval)
            .Append(lightManager.DirectionalFadeIn(1.5f))
            .Join(lightManager.PointFadeIn(1.5f))
            .Join(lightManager.SpotFadeOut(30f, 1f))
            .AppendCallback(() => input.EnqueueMessage(
                new MessageData[]
                {
                    new MessageData("『表の立て札は読まなかったのかい？』", FaceID.NONE),
                    new MessageData("いやまあ読んだけど・・・\n誰よ？あんた", FaceID.DEFAULT),
                    new MessageData("『私は迷宮の守護霊。その鍵は返してもらう。』", FaceID.NONE),
                    new MessageData("こっちだってコレないと外に出られないんだけど？？", FaceID.DESPISE),
                    new MessageData("『そっちの事情なんて知らないね。私はここの宝物を守るように命令を受けている。』", FaceID.NONE),
                    new MessageData("『ここで逃がすわけにはいかない・・・！』", FaceID.NONE),
                    new MessageData("ふーん・・・", FaceID.ASHAMED),
                    new MessageData("それ、誰の命令なんだろうね？", FaceID.EYECLOSE),
                    new MessageData("『誰・・・。誰って・・・・？』", FaceID.NONE),
                    new MessageData("知らないんだ？", FaceID.DISATTRACT2),
                    new MessageData("・・・まあ、私だってそっちの事情なんて知らんし", FaceID.DEFAULT),
                    new MessageData("こんなとこ、とっととトンズラさせてもらうわ！", FaceID.ANGRY),
                }
            ))
            .SetUpdate(false)
            .OnCompleteAsObservable(Unit.Default);
    }
}