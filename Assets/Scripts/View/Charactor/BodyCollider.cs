using UnityEngine;

public class BodyCollider
{
    protected CapsuleCollider col;
    protected float orgHeight { get; }
    protected float orgRadius { get; }
    protected Vector3 orgCenter { get; }

    public BodyCollider(CapsuleCollider col)
    {
        this.col = col;

        orgHeight = col.height;
        orgRadius = col.radius;
        orgCenter = col.center;
    }

    public void ResetCollider()
    {
        col.height = orgHeight;
        col.radius = orgRadius;
        col.center = orgCenter;
    }

}
