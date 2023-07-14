using UnityEngine;
using System;
using System.Linq;

public class FloorIllness
{
    private PlayerLifeGauge lifeGauge = default;
    private RestUI restUI = default;

    private Action[] illnessSetter;
    private Action[] illnessRemover;

    public FloorIllness(PlayerLifeGauge lifeGauge, RestUI restUI)
    {
        this.lifeGauge = lifeGauge;
        this.restUI = restUI;

        var numOfFloor = GameInfo.Instance.LastFloor;

        illnessSetter = Enumerable.Repeat<Action>(() => { }, numOfFloor).ToArray();
        illnessRemover = Enumerable.Repeat<Action>(() => { }, numOfFloor).ToArray();

        illnessSetter[2] = () => { restUI.SetPoison(); lifeGauge.SetHPColor(Color.red); };
        illnessRemover[2] = () => { restUI.RemovePoison(); lifeGauge.SetHPColor(Color.white); };

        illnessSetter[3] = () =>
        {
            restUI.SetCold();
            RenderSettings.fogColor = new Color(0.9f, 0.9f, 1f);
            RenderSettings.fogDensity = 0.075f;
            RenderSettings.fog = true;
        };
        illnessRemover[3] = () =>
        {
            restUI.RemoveCold();
            RenderSettings.fog = false;
        };
    }

    private int prevFloor = 1;

    public void SwitchFloor(int floor)
    {
        illnessRemover[prevFloor - 1]();
        illnessSetter[floor - 1]();
        prevFloor = floor;
    }
}