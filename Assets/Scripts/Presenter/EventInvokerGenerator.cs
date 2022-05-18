using UnityEngine;
using System;

public class EventInvokerGenerator : MobGenerator<EventInvoker>
{
    public virtual EventInvoker Spawn(Vector3 pos, Func<PlayerCommandTarget, bool> IsEventValid, bool isOneShot = true) => Spawn(pos).Init(IsEventValid, isOneShot);
}