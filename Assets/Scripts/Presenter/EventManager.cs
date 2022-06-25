using System;
using UnityEngine;
using UniRx;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class EventManager : MobGenerator<EventInvoker>
{
    [SerializeField] private PlayerCommandTarget playerTarget = default;
    [SerializeField] private LightManager lightManager = default;

    private IPlayerMapUtil map;
    private PlayerInput input;

    private int currentFloor = 0;

    private ICommand dropFloor;
    private ICommand turnL;
    private ICommand turnR;

    private Dictionary<Pos, EventInvoker>[] eventInvokers;

    public virtual EventInvoker Spawn(Pos pos, Func<PlayerCommandTarget, bool> IsEventValid, bool isOneShot = true) => Spawn().Init(pos, IsEventValid, isOneShot);

    protected override void Awake()
    {
        base.Awake();

        map = playerTarget.map as IPlayerMapUtil;
        input = playerTarget.input as PlayerInput;

        dropFloor = new PlayerDropFloor(playerTarget, 220f);
        turnL = new PlayerTurnL(playerTarget, 18f);
        turnR = new PlayerTurnR(playerTarget, 18f);
    }

    public void EventInit(WorldMap map)
    {
        eventInvokers = new Dictionary<Pos, EventInvoker>[GameInfo.Instance.LastFloor].Select(_ => new Dictionary<Pos, EventInvoker>()).ToArray();

        // KeyBlade detecting
        SetEvent(5, new Pos(11, 11),
            WitchGenerateEventIntro,
            // Invoke witch generating event if player has the KeyBlade or the KeyBlade is put on event tile
            target => target.itemInventory.hasKeyBlade() || target.map.OnTile.TopItem?.type == ItemType.KeyBlade || target.map.BackwardTile.TopItem?.type == ItemType.KeyBlade,
            true, Direction.south
        );

        SwitchWorldMap(map);
    }

    public void SwitchWorldMap(WorldMap map)
    {
        eventInvokers[map.floor - 1].ForEach(kv => kv.Value.Enable(map));
        if (currentFloor > 0) eventInvokers[currentFloor - 1].ForEach(kv => kv.Value.Enable(map));
        currentFloor = map.floor;
    }

    private void SetEvent(int floor, Pos pos, Action<IDirection> eventAct, Func<PlayerCommandTarget, bool> predicateEventValid, bool isOneShot = true, IDirection dir = null)
    {
        var eventInvoker = Spawn(pos, predicateEventValid, isOneShot);

        eventInvoker.DetectPlayer.Subscribe(_ => eventAct(dir), () => eventInvokers[floor - 1].Remove(pos)).AddTo(playerTarget.gameObject);

        eventInvokers[floor - 1][pos] = eventInvoker;
    }

    private void WitchGenerateEventIntro(IDirection witchDir)
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

        input.ObserveComplete(message)
            .Subscribe(null, () => WitchGenerateEventMain(witchDir))
            .AddTo(this);
    }

    private void WitchGenerateEventMain(IDirection witchDir)
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

        seq
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
            .Play();
    }

    public void DropStartEvent()
    {
        input.Interrupt(new PlayerDropFloor(playerTarget, 220f));
        input.EnqueueMessage(
            new MessageData[]
            {
                new MessageData("いきなりなんなのさ・・・", FaceID.DISATTRACT),
                new MessageData("久々の出番なのに、扱いが雑じゃない！？", FaceID.ANGRY)
            },
            false
        );

        if (map.IsExitDoorLeft)
        {
            input.EnqueueTurnR();
        }
        else if (map.IsExitDoorRight)
        {
            input.EnqueueTurnL();
        }
        else if (map.IsExitDoorBack)
        {
            input.EnqueueTurnL();
            input.EnqueueTurnL();
        }

        input.EnqueueMessage(
            new MessageData[]
            {
                new MessageData("なんか使う標示まちがってる気がするけど", FaceID.DEFAULT),
                new MessageData("どうみてもこれが出口だね", FaceID.NOTICE),
                new MessageData("・・・うーん", FaceID.DISATTRACT),
                new MessageData("鍵が掛かってますねぇ！", FaceID.DISATTRACT),
            }
        );
    }

    public void RestartEvent()
    {
        input.InterruptMessage(
            new MessageData[]
            {
                new MessageData("[仮] ・・・という夢だったのさ", FaceID.SMILE),
                new MessageData("[仮] なんも解決してないんだけどねっ！", FaceID.ANGRY)
            }
        );
    }
}
