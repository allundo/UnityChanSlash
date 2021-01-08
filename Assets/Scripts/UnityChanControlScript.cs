using UnityEngine;
using System;

// 必要なコンポーネントの列記
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]

public class UnityChanControlScript : MobControl
{
    public Point GetWorldPoint()
    {
        Vector3 pos = transform.position;
        return new Point { x = (int)Math.Round(pos.x, MidpointRounding.AwayFromZero), y = (int)Math.Round(pos.y, MidpointRounding.AwayFromZero) };
    }

    // 初期化
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }
}
