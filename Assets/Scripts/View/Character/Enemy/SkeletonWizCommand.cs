using DG.Tweening;

public class SkeletonWizCommand : EnemyTurnCommand
{
    protected IMagicianAnimator magicianAnim;
    protected SkeletonWizReactor skeletonWizReact;

    public SkeletonWizCommand(EnemyCommandTarget target, float duration, float validateTiming = 0.95f) : base(target, duration, validateTiming)
    {
        magicianAnim = target.anim as IMagicianAnimator;
        skeletonWizReact = target.react as SkeletonWizReactor;
    }
}

public class SkeletonWizTeleport : SkeletonWizCommand
{
    public SkeletonWizTeleport(EnemyCommandTarget target, float duration) : base(target, duration) { }

    protected override bool Action()
    {
        Pos destPos = map.SearchSpaceNearBy(3);

        if (destPos.IsNull) return false;

        magicianAnim.teleport.Bool = true;
        skeletonWizReact.OnTeleport(duration);

        completeTween = DOTween.Sequence()
            .Join(tweenMove.Teleport(destPos))
            .InsertCallback(0.50f * duration, () =>
            {
                magicianAnim.teleport.Bool = false;
                enemyMap.MoveOnEnemy(destPos);
                skeletonWizReact.OnTeleportDest();
            })
            .InsertCallback(duration, skeletonWizReact.OnTeleportEnd)
            .SetUpdate(false)
            .Play();

        return true;
    }
}
