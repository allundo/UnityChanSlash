using System;
using UnityEngine;
using UniRx;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class EventManager
{
    private IPlayerMapUtil map;
    private PlayerInput input;
    private LightManager lightManager;
    private EventInvokerGenerator eventInvokerGenerator;
    private int currentFloor = 0;

    private Dictionary<Pos, EventInvoker>[] eventInvokers;

    public EventManager(IPlayerMapUtil map, PlayerInput input, LightManager lightManager, EventInvokerGenerator eventInvokerGenerator)
    {
        this.map = map;
        this.input = input;
        this.lightManager = lightManager;
        this.eventInvokerGenerator = eventInvokerGenerator;

        EventInit();
    }

    private void EventInit()
    {
        eventInvokers = new Dictionary<Pos, EventInvoker>[GameInfo.Instance.LastFloor].Select(_ => new Dictionary<Pos, EventInvoker>()).ToArray();

        // KeyBlade detecting
        // Invoke witch generating event if player has the KeyBlade or the KeyBlade is put on event tile.
        SetEvent(5, new Pos(11, 11), WitchGenerateEvent, target => target.itemInventory.hasKeyBlade() || target.map.OnTile.TopItem?.type == ItemType.KeyBlade);
    }

    public void MoveFloor(int nextFloor)
    {
        eventInvokers[nextFloor - 1].ForEach(kv => kv.Value.SetEnabled(true));
        if (currentFloor > 0) eventInvokers[currentFloor - 1].ForEach(kv => kv.Value.SetEnabled(false));
        currentFloor = nextFloor;
    }

    private void SetEvent(int floor, Pos pos, Action eventAct, Func<PlayerCommandTarget, bool> isEventValid, bool isOneShot = true)
    {
        var eventInvoker = eventInvokerGenerator.Spawn(map.WorldPos(pos), isEventValid, isOneShot);

        eventInvoker.DetectPlayer.Subscribe(_ => eventAct(), () => eventInvokers[floor - 1].Remove(pos)).AddTo(input.gameObject);

        eventInvokers[floor - 1][pos] = eventInvoker;
    }

    private void WitchGenerateEvent()
    {
        input.SetInputVisible(false);

        lightManager.DirectionalFade(1f, 0.2f, 1.5f).SetEase(Ease.InCubic).Play();

        input.EnqueueStartMessage(
            new MessageData[]
            {
                new MessageData("『ちょっと待ちなよ』", FaceID.NONE),
                new MessageData("・・・！", FaceID.SURPRISE),
            },
            false
        );

        var spotTween = DOTween.Sequence()
            .AppendCallback(() => GameManager.Instance.PlaceEnemy(EnemyType.Witch, map.GetBackward, map.dir, new EnemyStatus.ActivateOption(3f, true)))
            .Append(lightManager.SpotFade(map.WorldPos(map.GetBackward) + new Vector3(0, 4f, 0), 1f, 30f, 2f))
            .SetUpdate(true);

        input.ForceEnqueue(new Command(input, DOVirtual.DelayedCall(0.2f, null, false), spotTween.SetDelay(0.2f)));

        input.EnqueueTurnL();
        input.EnqueueTurnL();

        var resumeTween = DOTween.Sequence()
            .Join(lightManager.DirectionalFade(0.2f, 1f, 2f))
            .Join(lightManager.SpotFadeOut(30f, 2f))
            .SetUpdate(false);

        input.ForceEnqueue(new Command(input, DOVirtual.DelayedCall(2f, null, false), resumeTween.SetDelay(2f)));

        input.EnqueueStartMessage(
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
        );
    }

    public void DropStartEvent()
    {
        input.EnqueueDropFloor();

        input.EnqueueStartMessage(
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

        input.EnqueueStartMessage(
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
        input.EnqueueRestartMessage(
            new MessageData[]
            {
                new MessageData("[仮] ・・・という夢だったのさ", FaceID.SMILE),
                new MessageData("[仮] なんも解決してないんだけどねっ！", FaceID.ANGRY)
            }
        );
    }
}
