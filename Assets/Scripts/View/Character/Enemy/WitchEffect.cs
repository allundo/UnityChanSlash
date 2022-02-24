public class WitchEffect : GhostEffect
{
    /// <summary>
    /// Stop charging trail and target attack trail OnDie()
    /// </summary>
    protected override void StopAnimFX()
    {
        OnAttackEnd();
        (animFX as WitchAnimFX).OnTrailEnd();
    }
}
