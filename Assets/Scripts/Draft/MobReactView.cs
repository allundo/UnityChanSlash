using System;
using UnityEngine;

public class MobReactView : MonoBehaviour
{

    public Action OnDamageListener = null;

    public void OnDamage(float power, IDirection dir)
    {
        if (OnDamageListener != null) OnDamageListener();
    }

}