using UnityEngine;
public class PlayerBodyCollider : BodyCollider
{
    public PlayerBodyCollider(CapsuleCollider col) : base(col) { }

    public void JumpCollider(float jumpHeight)
        => TransformCollider(jumpHeight, -0.5f, new Vector3(0f, 1f, 0f));

    public void OverRunCollider(float brakeOverRun)
        => TransformCollider(brakeOverRun, -0.05f, new Vector3(0f, -0.025f, 0.5f), 0.3f);

    public void IcedFallCollider(float fallHeight)
    {
        TransformCollider(fallHeight, 1.25f, new Vector3(0f, 0.625f, 0f), -0.5f);
    }

    protected void TransformCollider(float value, float stretchRate, Vector3 moveRate, float radiusRate = 0f)
    {
        if (value == 0f)
        {
            ResetCollider();
            return;
        }

        col.height = orgHeight + value * stretchRate;
        col.center = orgCenter + value * moveRate;
        col.radius = orgRadius + value * radiusRate;
    }
}
