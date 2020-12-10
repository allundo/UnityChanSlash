using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;


public class GameManager : SingletonMonoBehaviour<GameManager>
{
    public WorldMap worldMap { get; protected set; }
    protected override void Awake()
    {
        base.Awake();


        worldMap = new WorldMap(new Dungeon());
        MapRenderer.Instance.Fix(worldMap);

    }
}