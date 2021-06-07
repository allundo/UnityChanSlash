using UnityEngine;
public class PlayerBodyCollider : BodyCollider
{
    public PlayerBodyCollider(CapsuleCollider col) : base(col) { }

    public void JumpCollider(float jumpHeight)
    {
        if (jumpHeight < 0.0001f)
        {
            ResetCollider();
            return;
        }

        col.height = orgHeight - jumpHeight;
        col.center = new Vector3(0, orgCenter.y + jumpHeight, 0);
    }
}