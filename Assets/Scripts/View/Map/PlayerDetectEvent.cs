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
    protected Pos pos;
    public bool isOneShot { get; protected set; }

    protected abstract bool IsEventValid();

    public PlayerDetectEvent(PlayerCommandTarget target, Pos pos, bool isOneShot = true) : base(target)
    {
        this.pos = pos;
        this.isOneShot = isOneShot;
    }

    public void Activate(EventInvoker invoker, WorldMap map)
    {
        this.invoker = invoker.OnSpawn(map.WorldPos(pos));

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

public class WitchGenerateEvent : PlayerDetectEvent
{
    private LightManager lightManager;

    public WitchGenerateEvent(PlayerCommandTarget target, LightManager lightManager, Pos pos) : base(target, pos, true)
    {
        this.lightManager = lightManager;
    }

    protected override bool IsEventValid()
    {
        var playerHasKeyBlade = ItemInventory.Instance.hasKeyBlade();
        var keyBladeIsOnEventTile = target.map.OnTile.HasItem(ItemType.KeyBlade);
        var jumpLeapedEventTileWithKeyBlade = target.map.BackwardTile.HasItem(ItemType.KeyBlade);

        return playerHasKeyBlade || keyBladeIsOnEventTile || jumpLeapedEventTileWithKeyBlade;
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