using System;
using UnityEngine;

[RequireComponent(typeof(EnemyCommander))]
public class EnemyControl : MonoBehaviour
{

    protected EnemyCommander commander;

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