using System.Linq;
using UniRx;
using System;

public class AnnaSealDoorsEvent
{
    protected PlaceEnemyGenerator placeEnemyGenerator;
    protected Pos[] eventPos;
    protected IEventHandleState[] eventStates = null;

    public AnnaSealDoorsEvent(PlaceEnemyGenerator placeEnemyGenerator)
    {
        this.placeEnemyGenerator = placeEnemyGenerator;

        // Event door tile positions
        eventPos = new Pos[] { new Pos(19, 27), new Pos(12, 23), new Pos(27, 24), new Pos(20, 25), new Pos(24, 25) };
    }

    public void SwitchWorldMap(WorldMap map)
    {
        if (map.floor == 5)
        {
            // Load event tile states
            eventStates = eventStates ?? eventPos.Select(pos => (map.GetTile(pos) as IEventTile).eventState).ToArray();

            var annaStatus = placeEnemyGenerator.GetAnnaStatus();
            if (annaStatus != null) EventOn(annaStatus);
        }
    }

    public void StartEvent(IEnemyStatus annaStatus)
    {
        eventStates.ForEach(state => state.EventOn());

        // Wait for closing sealed door.
        Observable.Timer(TimeSpan.FromSeconds(1f))
            .Subscribe(_ => placeEnemyGenerator.EraseInvisibleEnemies())
            .AddTo(annaStatus.gameObject);

        EventOn(annaStatus);
    }

    private void EventOn(IEnemyStatus annaStatus)
    {
        placeEnemyGenerator.DisableAllEnemyGenerators();
        ReserveEventOff(annaStatus);
    }

    private void ReserveEventOff(IEnemyStatus annaStatus)
    {
        annaStatus.Life.Where(life => life == 0f)
            .Subscribe(_ =>
            {
                eventStates.ForEach(state => state.EventOff());
                placeEnemyGenerator.EnableAllEnemyGenerators();
            })
            .AddTo(annaStatus.gameObject);
    }
}
