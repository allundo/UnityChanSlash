using System;
using System.Linq;
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

    protected abstract bool IsEventValid(WorldMap map);

    public PlayerDetectEvent(PlayerInput input, bool isOneShot = true) : base(input)
    {
        this.isOneShot = isOneShot;
    }

    protected abstract Vector3 EventTilePosition(WorldMap map);

    public void Activate(EventInvoker invoker, WorldMap map)
    {
        this.invoker = invoker.OnSpawn(EventTilePosition(map));

        disposable = invoker.DetectPlayer
            .Where(_ => IsEventValid(map))
            .SelectMany(_ => Invoke())
            .Subscribe(_ =>
            {
                if (isOneShot)
                {
                    eventEndSubject.OnCompleted();
                    Inactivate();
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
    protected MessageData eventMessages;

    public PlayerDetectFloor(PlayerInput input, MessageData eventMessages) : base(input)
    {
        this.eventMessages = eventMessages;
    }

    protected override bool IsEventValid(WorldMap map) => true;
    protected override Vector3 EventTilePosition(WorldMap map) => map.WorldPos(map.stairsBottom.Key);

    protected override IObservable<Unit> EventFunc()
        => input
            .ObserveComplete(input.EnqueueMessage(eventMessages))
            .Select(_ => Unit.Default);
}

public class PlayerMessageFloor3 : PlayerDetectFloor
{
    public PlayerMessageFloor3(PlayerInput input) : base(input, new MessageData
    (
        new MessageSource("そろそろ地下 3 階ってもんかしら？\n…なんかめっちゃ暑いんですけど。", FaceID.DEFAULT),
        new MessageSource("体力が奪われてく感じがする…。\nこまめに休息とらないとしんどいね、こりゃ。", FaceID.DESPISE)
    ))
    { }
}
public class PlayerMessageFloor4 : PlayerDetectFloor
{
    public PlayerMessageFloor4(PlayerInput input) : base(input, new MessageData
    (
        new MessageSource("いよいよ地下 4 階！\n・・・って、寒ゥ！！！", FaceID.SURPRISE),
        new MessageSource("フィンランド式サウナか！\n一体どうなってんの？！", FaceID.ANGRY),
        new MessageSource("ここで寝たらアウトだね…。\n急いで突破しよ！", FaceID.ANGRY2)
    ))
    { }
}

public class PlayerMessageFloor5 : PlayerDetectFloor
{
    public PlayerMessageFloor5(PlayerInput input) : base(input, new MessageData
    (
        new MessageSource("地下 5 階とーちゃーく！\nうぇ〜寒かったぁ…。", FaceID.SMILE),
        new MessageSource("…なんだかモノモノシイ雰囲気。\nこれが最後って感じかな？", FaceID.EYECLOSE),
        new MessageSource("とっとと鍵奪って脱出してやりますかね！", FaceID.ANGRY2)
    ))
    { }
}

public abstract class PlayerHasItemEvent : PlayerDetectEvent
{
    protected Pos pos;
    protected ItemType itemTypeToCheck;

    public PlayerHasItemEvent(PlayerInput input, Pos pos, ItemType itemTypeToCheck, bool isOneShot = true) : base(input, isOneShot)
    {
        this.itemTypeToCheck = itemTypeToCheck;
        this.pos = pos;
    }

    protected override Vector3 EventTilePosition(WorldMap map) => map.WorldPos(pos);

    protected override bool IsEventValid(WorldMap map)
    {
        var playerHasItem = ItemInventory.Instance.hasItemType(itemTypeToCheck);
        var itemIsOnEventTile = input.playerMap.OnTile.HasItem(itemTypeToCheck);
        var jumpLeapedEventTileWithItem = input.playerMap.BackwardTile.HasItem(itemTypeToCheck);

        return playerHasItem || itemIsOnEventTile || jumpLeapedEventTileWithItem;
    }
}

public abstract class SimpleEnemyGenerateEvent : PlayerDetectEvent
{
    protected Pos detectTilePos;
    protected EnemyType type;
    protected Pos spawnTilePos;
    protected EnemyStatus.ActivateOption option;
    protected EnemyStoreData data;

    public SimpleEnemyGenerateEvent(PlayerInput input, Pos detectTilePos, EnemyType type, Pos spawnTilePos, int level, bool isOneShot = false) : base(input, isOneShot)
    {
        this.detectTilePos = detectTilePos;
        this.type = type;
        this.spawnTilePos = spawnTilePos;
        this.option = new EnemyStatus.ActivateOption();
        this.data = new EnemyStoreData(level - 1);
    }

    protected override Vector3 EventTilePosition(WorldMap map) => map.WorldPos(detectTilePos);
    protected override IObservable<Unit> EventFunc()
    {
        SpawnHandler.Instance.PlaceEnemy(type, spawnTilePos, Direction.north, option, data);
        return Observable.NextFrame();
    }
}

public class AnnaGenerateEvent : SimpleEnemyGenerateEvent
{
    protected AnnaSealDoorsEvent annaSealDoorsEvent;
    public AnnaGenerateEvent(PlayerInput input, Pos detectTilePos, AnnaSealDoorsEvent annaSealDoorsEvent)
        : base(input, detectTilePos, EnemyType.Anna, new Pos(13, 23), 1, true)
    {
        this.annaSealDoorsEvent = annaSealDoorsEvent;
    }

    protected override bool IsEventValid(WorldMap map) => true;

    protected override IObservable<Unit> EventFunc()
    {
        annaSealDoorsEvent.StartEvent(SpawnHandler.Instance.PlaceEnemy(type, spawnTilePos, Direction.east, option, data));
        return Observable.NextFrame();
    }
}

public class SkeletonWizardGenerateEvent : SimpleEnemyGenerateEvent
{
    public SkeletonWizardGenerateEvent(PlayerInput input, Pos detectTilePos)
        : base(input, detectTilePos, EnemyType.SkeletonWizard, new Pos(3, 17), 4) { }

    protected override bool IsEventValid(WorldMap map)
    {
        bool isDoorClose = !(map.GetTile(3, 18) as Door).IsOpen;
        bool isSkeletonOff = !map.GetTile(3, 17).IsEnemyOn;
        return isDoorClose && isSkeletonOff;
    }
}

public class RedSlimeGenerateEvent : SimpleEnemyGenerateEvent
{
    public RedSlimeGenerateEvent(PlayerInput input, Pos detectTilePos)
        : base(input, detectTilePos, EnemyType.RedSlime, new Pos(7, 17), 20) { }

    protected override bool IsEventValid(WorldMap map)
    {
        bool isDoorClose = !(map.GetTile(3, 18) as Door).IsOpen;
        bool isEnemyOff = !map.GetTile(7, 17).IsEnemyOn;
        return isDoorClose && isEnemyOff;
    }
}

public class SkeletonsGenerateEvent : PlayerHasItemEvent
{
    public SkeletonsGenerateEvent(PlayerInput input, Pos pos) : base(input, pos, ItemType.TreasureKey) { }

    protected override IObservable<Unit> EventFunc()
    {
        var spawn = SpawnHandler.Instance;
        var option = new EnemyStatus.ActivateOption();

        spawn.PlaceEnemy(EnemyType.SkeletonSoldier, new Pos(21, 3), Direction.south, option);
        spawn.PlaceEnemy(EnemyType.SkeletonSoldier, new Pos(21, 17), Direction.north, option);
        spawn.PlaceEnemy(EnemyType.SkeletonWizard, new Pos(24, 9), Direction.west, option);
        spawn.PlaceEnemy(EnemyType.SkeletonSoldier, new Pos(24, 11), Direction.west, option);
        spawn.PlaceEnemy(EnemyType.SkeletonWizard, new Pos(24, 13), Direction.west, option);
        spawn.PlaceEnemy(EnemyType.SkeletonSoldier, new Pos(24, 15), Direction.west, option);
        spawn.PlaceEnemy(EnemyType.SkeletonWizard, new Pos(24, 17), Direction.west, option);

        ActiveMessageController.Instance.InputMessageData("スケルトンがいっぱい！", SDFaceID.SURPRISE, SDEmotionID.EXSURPRISE);

        var map = GameManager.Instance.worldMap;

        Door door = map.GetTile(21, 18) as Door;
        if (door.IsOpen && !door.IsBroken) door.Handle();

        new Pos[] { new Pos(22, 9), new Pos(22, 11), new Pos(22, 13), new Pos(22, 15), new Pos(22, 17) }
            .Select(pos => (map.GetTile(pos) as IEventTile).eventState)
            .ForEach(state => state.EventOn());

        GameManager.Instance.RedrawPlates();
        return Observable.NextFrame();
    }
}

public class WitchGenerateEvent : PlayerHasItemEvent
{
    private LightManager lightManager;
    private IPlayerMapUtil map;

    public WitchGenerateEvent(PlayerInput input, LightManager lightManager, Pos pos) : base(input, pos, ItemType.KeyBlade)
    {
        this.lightManager = lightManager;
        map = input.playerMap;
    }

    protected override IObservable<Unit> EventFunc()
    {
        input.InputStop();
        input.SetInputVisible(false);

        SpawnHandler.Instance.EraseAllEnemies();

        ICommand message = input.EnqueueMessage(
            new MessageData
            (
                new MessageSource("『ちょっと待ちなよ』", FaceID.NONE),
                new MessageSource("・・・！", FaceID.SURPRISE)
            ),
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
                new MessageData
                (
                    new MessageSource("『表の立て札は読まなかったのかい？』", FaceID.NONE),
                    new MessageSource("いやまあ読んだけど・・・\n誰よ？あんた", FaceID.DEFAULT),
                    new MessageSource("『私は迷宮の守護霊。その鍵は返してもらう。』", FaceID.NONE),
                    new MessageSource("こっちだってコレないと外に出られないんだけど？？", FaceID.DESPISE),
                    new MessageSource("『そっちの事情なんて知らないね。私はここの宝物を守るように命令を受けている。』", FaceID.NONE),
                    new MessageSource("『ここで逃がすわけにはいかない・・・！』", FaceID.NONE),
                    new MessageSource("ふーん・・・", FaceID.ASHAMED),
                    new MessageSource("それ、誰の命令なんだろうね？", FaceID.EYECLOSE),
                    new MessageSource("『誰・・・。誰って・・・・？』", FaceID.NONE),
                    new MessageSource("知らないんだ？", FaceID.DISATTRACT2),
                    new MessageSource("・・・まあ、私だってそっちの事情なんて知らんし", FaceID.DEFAULT),
                    new MessageSource("こんなとこ、とっととトンズラさせてもらうわ！", FaceID.ANGRY)
                )
            ))
            .SetUpdate(false)
            .OnCompleteAsObservable(Unit.Default);
    }
}