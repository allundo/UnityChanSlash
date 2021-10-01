using UnityEngine;
using UniRx;

public abstract class EnemyCommand : Command
{
    protected EnemyAnimator enemyAnim;

    public EnemyCommand(EnemyCommander commander, float duration) : base(commander, duration)
    {
        enemyAnim = anim as EnemyAnimator;
    }
}

public abstract class EnemyMove : EnemyCommand
{
    public EnemyMove(EnemyCommander commander, float duration) : base(commander, duration) { }

    protected abstract bool IsMovable { get; }
    protected abstract Vector3 Dest { get; }
    protected Vector3 startPos = default;
    protected void SetSpeed()
    {
        enemyAnim.speed.Float = Speed;
    }

    protected void ResetSpeed()
    {
        enemyAnim.speed.Float = 0.0f;
    }

    public override void Cancel()
    {
        base.Cancel();
        map.ResetOnCharactor(startPos + Dest);
    }

    public override void Execute()
    {
        if (!IsMovable)
        {
            onValidateInput.OnNext(true);
            onCompleted.OnNext(Unit.Default);
            return;
        }

        startPos = map.CurrentVec3Pos;
        map.SetOnCharactor(startPos + Dest);
        map.ResetOnCharactor(startPos);

        SetSpeed();
        PlayTween(tweenMove.GetLinearMove(Dest), () => ResetSpeed());

        SetValidateTimer(0.95f);
    }
}

public class EnemyForward : EnemyMove
{
    public EnemyForward(EnemyCommander commander, float duration) : base(commander, duration) { }

    protected override bool IsMovable => map.IsForwardMovable;
    protected override Vector3 Dest => map.GetForwardVector();
    public override float Speed => TILE_UNIT / duration;
}

public class EnemyTurnL : EnemyCommand
{
    public EnemyTurnL(EnemyCommander commander, float duration) : base(commander, duration) { }

    public override void Execute()
    {
        PlayTween(tweenMove.GetRotate(-90));
        map.TurnLeft();

        SetValidateTimer();
    }
}

public class EnemyTurnR : EnemyCommand
{
    public EnemyTurnR(EnemyCommander commander, float duration) : base(commander, duration) { }

    public override void Execute()
    {
        PlayTween(tweenMove.GetRotate(90));
        map.TurnRight();

        SetValidateTimer();
    }
}
public class EnemyAttack : EnemyCommand
{
    private MobAttack enemyAttack;
    public EnemyAttack(EnemyCommander commander, float duration) : base(commander, duration)
    {
        enemyAttack = commander.enemyAttack;
    }

    public override void Execute()
    {
        enemyAnim.attack.Fire();
        playingTween = enemyAttack.SetAttack(duration);

        SetValidateTimer();
        SetDispatchFinal();
    }
}
