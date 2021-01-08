using System;
using UnityEngine;

[RequireComponent(typeof(Commander))]
public class MobControl : MonoBehaviour
{

    protected Commander commander;

    protected Vector3 prevPosition;
    protected Point WorldPos
    {
        get
        {
            Vector3 pos = transform.position;
            return new Point { x = (int)Math.Round(pos.x, MidpointRounding.AwayFromZero), y = (int)Math.Round(pos.y, MidpointRounding.AwayFromZero) };
        }
    }

    public float GetSpeed()
    {
        float speed = (transform.position - prevPosition).magnitude / Time.deltaTime;
        prevPosition = transform.position;

        return speed;
    }

    protected virtual void Start()
    {
        commander = GetComponent<Commander>();
        prevPosition = transform.position;
    }

    protected virtual void Update()
    {
        commander.InputCommand();
    }

}