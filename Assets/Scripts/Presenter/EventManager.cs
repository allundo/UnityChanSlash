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
    [SerializeField] private MessageController messageController = default;
    [SerializeField] private GameOverUI gameOverUI = default;

    private EventCommandTarget target;

    private IPlayerMapUtil map;
    private PlayerInput input;

    private int currentFloor = 0;

    private Commander commander;

    private ICommand dropFloor;
    private ICommand turnL;
    private ICommand turnR;

    private Dictionary<Pos, EventInvoker>[] eventInvokers;

    public virtual EventInvoker Spawn(Vector3 pos, Func<PlayerCommandTarget, bool> IsEventValid, bool isOneShot = true) => Spawn(pos).Init(IsEventValid, isOneShot);

    protected override void Awake()
    {
        base.Awake();
        target = new EventCommandTarget(playerTarget, messageController, gameOverUI, lightManager);
        commander = new Commander(playerTarget.gameObject);

        map = target.map as IPlayerMapUtil;
        input = target.input as PlayerInput;

        dropFloor = new PlayerDropFloor(playerTarget, 220f);
        turnL = new PlayerTurnL(playerTarget, 18f);
        turnR = new PlayerTurnR(playerTarget, 18f);
    }

    public void EventInit(int firstFloor)
    {
        eventInvokers = new Dictionary<Pos, EventInvoker>[GameInfo.Instance.LastFloor].Select(_ => new Dictionary<Pos, EventInvoker>()).ToArray();

        // KeyBlade detecting
        SetEvent(5, new Pos(11, 11),
            WitchGenerateEvent,
            // Invoke witch generating event if player has the KeyBlade or the KeyBlade is put on event tile
            target => target.itemInventory.hasKeyBlade() || target.map.OnTile.TopItem?.type == ItemType.KeyBlade || target.map.BackwardTile.TopItem?.type == ItemType.KeyBlade,
            true, Direction.south
        );

        MoveFloor(firstFloor);
    }

    private void Enqueue(ICommand cmd) => commander.EnqueueCommand(cmd);
    private void WaitEnqueue(ICommand cmd)
        => DOVirtual.DelayedCall(input.RemainingDuration, () => commander.EnqueueCommand(cmd), false).Play();

    private void Interrupt(ICommand cmd) => commander.Interrupt(cmd);

    public void EnqueueMessage(MessageData[] data, bool isUIVisibleOnCompleted = true)
        => Enqueue(new EventMessage(target, data, isUIVisibleOnCompleted));

    public void WaitEnqueueMessage(MessageData[] data, bool isUIVisibleOnCompleted = true)
        => WaitEnqueue(new EventMessage(target, data, isUIVisibleOnCompleted));

    public void InterruptMessage(MessageData[] data)
        => Interrupt(new EventMessage(target, data));

    public void MoveFloor(int nextFloor)
    {
        eventInvokers[nextFloor - 1].ForEach(kv => kv.Value.SetEnabled(true));
        if (currentFloor > 0) eventInvokers[currentFloor - 1].ForEach(kv => kv.Value.SetEnabled(false));
        currentFloor = nextFloor;
    }

    private void SetEvent(int floor, Pos pos, Action<IDirection> eventAct, Func<PlayerCommandTarget, bool> isEventValid, bool isOneShot = true, IDirection dir = null)
    {
        var eventInvoker = Spawn(map.WorldPos(pos), isEventValid, isOneShot);

        eventInvoker.DetectPlayer.Subscribe(_ => eventAct(dir), () => eventInvokers[floor - 1].Remove(pos)).AddTo(playerTarget.gameObject);

        eventInvokers[floor - 1][pos] = eventInvoker;
    }

    private void WitchGenerateEvent(IDirection witchDir)
    {
        input.InputStop();
        input.SetInputVisible(false);

        input.EnqueueMessage(
            new MessageData[]
            {
                new MessageData("『ちょっと待ちなよ』", FaceID.NONE),
                new MessageData("・・・！", FaceID.SURPRISE),
            },
            false
        );

        WaitEnqueue(new WitchGenerateEvent(target, witchDir));
    }

    public void DropStartEvent()
    {
        Interrupt(new PlayerDropFloor(playerTarget, 220f));
        EnqueueMessage(
            new MessageData[]
            {
                new MessageData("いきなりなんなのさ・・・", FaceID.DISATTRACT),
                new MessageData("久々の出番なのに、扱いが雑じゃない！？", FaceID.ANGRY)
            },
            false
        );

        if (map.IsExitDoorLeft)
        {
            Enqueue(turnR);
        }
        else if (map.IsExitDoorRight)
        {
            Enqueue(turnL);
        }
        else if (map.IsExitDoorBack)
        {
            Enqueue(turnL);
            Enqueue(turnL);
        }

        EnqueueMessage(
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
        InterruptMessage(
            new MessageData[]
            {
                new MessageData("[仮] ・・・という夢だったのさ", FaceID.SMILE),
                new MessageData("[仮] なんも解決してないんだけどねっ！", FaceID.ANGRY)
            }
        );
    }
}
