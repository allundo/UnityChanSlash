using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class LightLaserAttack : MagicAttack
{
    [SerializeField] private float colliderSizeRatio = 1f;

    public void SetColliderLength()
    {
        float length = (status as ILaserStatus).length * Constants.TILE_UNIT;

        var laserCollider = attackCollider as BoxCollider;

        laserCollider.center = new Vector3(0f, 1f, (length + 1.0f) * 0.5f);
        laserCollider.size = new Vector3(2f, 2f, length * colliderSizeRatio);
    }
}
