using UnityEngine;

[RequireComponent(typeof(BulletReactor))]
[RequireComponent(typeof(BulletInput))]
public class BulletCommandTarget : CommandTarget
{

    // Bullet attack handler for BulletAttack Command execution.
    [SerializeField] public Attack attack = default;
}
