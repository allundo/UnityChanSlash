using UnityEngine;
using UniRx;
using System;

[RequireComponent(typeof(MobEffect))]
public class EnemyReactor : MobReactor
{
    private static readonly Vector3 OUT_OF_SCREEN = new Vector3(1024.0f, 0.0f, 1024.0f);

    private IDisposable inactiveNextFrame;

    /// <summary>
    /// Before being dead, enemies must move out of player's detection on Minimap <br />
    /// since EnemySymbol inactivated OnTriggerExit of the detection.
    /// </summary>
    protected override void OnDead()
    {
        // Force TriggerExit from enemy detecting collider
        bodyCollider.enabled = true;
        transform.position = OUT_OF_SCREEN;

        // Wait for the TriggerExit event firing before inactivating the enemy
        inactiveNextFrame = Observable.NextFrame().Subscribe(_ => Inactivate());
    }

    private void Inactivate()
    {
        input.ClearAll();
        bodyCollider.enabled = false;
        status.Inactivate();
    }

    public void Disappear()
    {
        input.ClearAll(true, false);
        map.ResetTile();
        FadeOutToDead();
    }

    public override void Destroy()
    {
        inactiveNextFrame?.Dispose();
        base.Destroy();
    }
}
