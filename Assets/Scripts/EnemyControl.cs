using System;
using UnityEngine;

public class EnemyControl : MonoBehaviour
{

    [SerializeField] public MobCommander commander;

    protected virtual void Start()
    {
        commander = GetComponent<EnemyCommander>();
    }

    protected virtual void Update()
    {
        commander.InputCommand();
        commander.SetSpeed();
    }

}