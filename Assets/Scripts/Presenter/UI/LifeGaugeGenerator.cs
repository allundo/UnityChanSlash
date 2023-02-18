using UniRx;
using System.Collections.Generic;

public class LifeGaugeGenerator : MobGenerator<PanelLifeGauge>
{
    private Dictionary<IEnemyStatus, PanelLifeGauge> gauges = new Dictionary<IEnemyStatus, PanelLifeGauge>();

    public void Show(IEnemyStatus status) => gauges.LazyLoad(status, Spawn);
    public void Hide(IEnemyStatus status)
    {
        PanelLifeGauge gauge;
        if (gauges.TryGetValue(status, out gauge)) gauge.Disable();
    }

    private PanelLifeGauge Spawn(IEnemyStatus status)
    {
        var gauge = Spawn(status.corePos, PlayerInfo.Instance.Dir).SetStatus(status);

        gauge.DetectDisable
            .Subscribe(status => gauges.Remove(status))
            .AddTo(gameObject);

        return gauge;
    }
}
