using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class LightLaserAttack : MagicAttack
{
    [SerializeField] private float colliderSizeRatio = 1f;

    public void SetColliderLength()
    {
        float TILE_UNIT = Constants.TILE_UNIT;
        int length = (status as ILaserStatus).length;

        var laserCollider = attackCollider as BoxCollider;

        laserCollider.center = new Vector3(0f, 1f, (length + 1) * TILE_UNIT * 0.5f);
        laserCollider.size = new Vector3(2f, 2f, length * TILE_UNIT * colliderSizeRatio);
    }

    public IDirection dir => status.dir;
}
