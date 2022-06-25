﻿using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MiniMapHandler : MonoBehaviour
{
    [SerializeField] private MiniMap miniMap = default;
    private Collider enemyDetector;

    void Awake()
    {
        enemyDetector = GetComponent<Collider>();
    }

    public void SwitchWorldMap(WorldMap map) => miniMap.SwitchWorldMap(map);

    public void OnStartFloor()
    {
        miniMap.UpdateMiniMap(transform.position);
        miniMap.enabled = true;
        enemyDetector.enabled = true;
    }

    public void OnMoveFloor()
    {
        miniMap.enabled = false;
        enemyDetector.enabled = false;
    }

    public void UpdateMiniMap() => miniMap.UpdateMiniMap(transform.position);
    public PlayerSymbol Turn(IDirection dir) => miniMap.Turn(dir);

    private void OnTriggerEnter(Collider col)
    {
        miniMap.OnEnemyFind(col);
    }

    private void OnTriggerExit(Collider col)
    {
        miniMap.OnEnemyLeft(col);
    }
}
