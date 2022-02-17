public class GhostAnimator : FlyingAnimator
{
    public AnimatorBool wallThrough { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        wallThrough = new AnimatorBool(anim, "WallThrough");
    }
}
