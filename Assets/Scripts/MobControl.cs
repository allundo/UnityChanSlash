using System;
using UnityEngine;

[RequireComponent(typeof(Commander))]
public class MobControl : MonoBehaviour
{

    protected Commander commander;

    protected virtual void Start()
    {
        commander = GetComponent<Commander>();
    }

    protected virtual void Update()
    {
        commander.InputCommand();
    }

}