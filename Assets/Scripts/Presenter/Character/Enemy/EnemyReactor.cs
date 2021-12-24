using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(MobEffect))]
public class EnemyReactor : MobReactor
{
    private static readonly Vector3 OUT_OF_SCREEN = new Vector3(1024.0f, 0.0f, 1024.0f);

    /// <summary>
    /// Before being dead, enemies must move out of player's detection on Minimap <br />
    /// since EnemySymbol inactivated OnTriggerExit of the detection.
    /// </summary>
    protected override void Dead()
    {
        // Force TriggerExit from enemy detecting collider
        transform.position = OUT_OF_SCREEN;

        // Wait for the TriggerExit event firing before inactivating the enemy
        DOVirtual.DelayedCall(0.01f, Inactivate, false).Play();
    }
}
