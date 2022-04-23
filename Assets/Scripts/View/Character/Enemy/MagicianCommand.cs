using DG.Tweening;

public class MagicianCommand : EnemyTurnCommand
{
    protected IMagicianAnimator magicianAnim;
    protected IMagicianReactor magicianReact;

    public MagicianCommand(EnemyCommandTarget target, float duration, float validateTiming = 0.95f) : base(target, duration, validateTiming)
    {
        magicianAnim = target.anim as IMagicianAnimator;
        magicianReact = target.react as IMagicianReactor;
    }
}

public class MagicianTeleport : MagicianCommand
{
    protected int range;
    public MagicianTeleport(EnemyCommandTarget target, float duration, int teleportRange = 2) : base(target, duration)
    {
        range = teleportRange;
    }

    protected override bool Action()
    {
        Pos destPos = enemyMap.SearchSpaceNearBy(range);

        if (destPos.IsNull) return false;

        magicianAnim.teleport.Bool = true;
        magicianReact.OnTeleport(duration);

        completeTween = DOTween.Sequence()
            .Join(tweenMove.Teleport(destPos))
            .InsertCallback(0.125f * duration, () => magicianReact.OnHide())
            .InsertCallback(0.50f * duration, () =>
            {
                magicianAnim.teleport.Bool = false;
                magicianReact.OnAppear();
                enemyMap.MoveOnEnemy(destPos);
                magicianReact.OnTeleportDest();
            })
            .InsertCallback(duration, magicianReact.OnTeleportEnd)
            .SetUpdate(false)
            .Play();

        return true;
    }

    public override ICommand GetContinuation()
        => new Command(input, null, completeTween, onCompleted); // Don't pause completeTween on iced.
}
