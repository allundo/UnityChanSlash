using System;
using UnityEngine;
using UniRx;
using System.Collections.Generic;
using System.Linq;

public class EventManager : MobGenerator<EventInvoker>
{
    [SerializeField] private PlayerCommandTarget playerTarget = default;
    [SerializeField] private LightManager lightManager = default;

    private int currentFloor = 0;
    public int currentEvent { get; private set; } = -1;

    private List<PlayerDetectEvent>[] playerDetectEvents;
    private List<GameEvent> gameEvents;

    protected override void Awake()
    {
        base.Awake();

        // Register game events
        gameEvents = new List<GameEvent>()
        {
            new DropStartEvent(playerTarget),
            new RestartEvent(playerTarget),
            new PlayerMessageFloor3(playerTarget),
            new PlayerMessageFloor4(playerTarget),
            new PlayerMessageFloor5(playerTarget),
            new WitchGenerateEvent(playerTarget, lightManager, new Pos(11, 11)),
            new SkeletonsGenerateEvent(playerTarget, new Pos(21, 7)),
            new SkeletonWizardGenerateEvent(playerTarget, new Pos(1, 11)),
            new RedSlimeGenerateEvent(playerTarget, new Pos(1, 13)),
        };
    }

    public void EventInit(WorldMap map)
    {
        // Initialize GameEvents invoked by detecting player
        playerDetectEvents = new List<PlayerDetectEvent>[GameInfo.Instance.LastFloor].Select(_ => new List<PlayerDetectEvent>()).ToArray();
        SetPlayerDetectEvent(3, 2);
        SetPlayerDetectEvent(4, 3);
        SetPlayerDetectEvent(5, 4);
        SetPlayerDetectEvent(5, 5);
        SetPlayerDetectEvent(5, 6);
        SetPlayerDetectEvent(5, 7);
        SetPlayerDetectEvent(5, 8);

        SwitchWorldMap(map);
    }

    private void SetPlayerDetectEvent(int floor, int gameEventIndex)
    {
        var gameEvent = gameEvents[gameEventIndex] as PlayerDetectEvent;
        if (gameEvent == null) throw new InvalidCastException("PlayerDetectEvent ではありません");
        playerDetectEvents[floor - 1].Add(gameEvent);

        if (!gameEvent.isOneShot) return;

        gameEvent.EventEndDetector
            .IgnoreElements()
            .Subscribe(null, () => playerDetectEvents[floor - 1].Remove(gameEvent))
            .AddTo(this);
    }

    public void SwitchWorldMap(WorldMap map)
    {
        if (currentFloor > 0) playerDetectEvents[currentFloor - 1].ForEach(evt => evt.Inactivate());

        playerDetectEvents[map.floor - 1].ForEach(evt => evt.Activate(Spawn(), map));

        currentFloor = map.floor;
    }

    public void RestartCurrentEvent()
    {
        if (currentEvent == -1)
        {
            DataStoreAgent.Instance.EnableSave();
            return;
        }

        InvokeGameEvent(currentEvent, true);
    }

    public void InvokeGameEvent(int gameEventIndex, bool isRestart = false)
    {
        if (gameEventIndex < 0 || gameEventIndex >= gameEvents.Count)
        {
            throw new IndexOutOfRangeException("GameEvent の登録がありません: " + gameEventIndex);
        }

        currentEvent = gameEventIndex;

        var dataStoreAgent = DataStoreAgent.Instance;
        dataStoreAgent.SaveCurrentGameData();
        dataStoreAgent.DisableSave();

        gameEvents[gameEventIndex].Invoke(isRestart)
            .IgnoreElements()
            .Subscribe(null, () =>
            {
                currentEvent = -1;
                dataStoreAgent.EnableSave();
            })
            .AddTo(this);
    }

    public DataStoreAgent.EventData[] ExportEventData()
    {
        return playerDetectEvents.Select(eventList => new DataStoreAgent.EventData(eventList.Select(evt => gameEvents.IndexOf(evt)).ToArray())).ToArray();
    }

    public void RespawnGameEvents(int currentEvent, DataStoreAgent.EventData[] eventData, WorldMap map)
    {
        this.currentEvent = currentEvent;

        playerDetectEvents = new List<PlayerDetectEvent>[GameInfo.Instance.LastFloor].Select(_ => new List<PlayerDetectEvent>()).ToArray();
        for (int i = 0; i < eventData.Length; i++)
        {
            eventData[i].eventList.ForEach(index => SetPlayerDetectEvent(i + 1, index));
        }

        SwitchWorldMap(map);
    }
}
