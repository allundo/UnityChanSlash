using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;

[RequireComponent(typeof(Animator))]
public class EnemyCommander : MobCommander
{
    protected Command moveForward;
    protected Command turnL;
    protected Command turnR;
    protected Command attack;

    protected bool IsPlayerFound(Pos pos)
    {
        Pos forward = dir.GetForward(pos);

        return IsOnPlayer(forward)
            ? true
            : !IsMovable(forward)
                ? false
                : IsPlayerFound(forward);
    }

    protected override void SetCommands()
    {
        moveForward = new ForwardCommand(this, 2.0f);
        turnL = new TurnLCommand(this, 0.1f);
        turnR = new TurnRCommand(this, 0.1f);
        attack = new AttackCommand(this, 2.0f);
        die = new DieCommand(this, 3.0f);
    }

    protected override Command GetCommand()
    {
        Pos pos = CurrentPos;

        // Turn if player found at left, right or backward
        Pos left = dir.GetLeft(pos);
        Pos right = dir.GetRight(pos);

        if (IsOnPlayer(left)) return turnL;
        if (IsOnPlayer(right)) return turnR;

        Pos backward = dir.GetBackward(pos);

        if (IsOnPlayer(backward))
        {
            return Random.Range(0, 2) == 0 ? turnL : turnR;
        }

        // Attack if player found at forward
        Pos forward = dir.GetForward(pos);
        if (IsOnPlayer(forward)) return attack;

        // Move forward if player found in front
        if (IsPlayerFound(forward)) return moveForward;

        bool isForwardMovable = IsMovable(forward);
        bool isLeftMovable = IsMovable(left);
        bool isRightMovable = IsMovable(right);

        if (isForwardMovable)
        {
            // Turn 50% if left or right movable
            if (Random.Range(0, 2) == 0)
            {
                if (Random.Range(0, 2) == 0)
                {
                    if (currentCommand == turnR) return moveForward;
                    if (isLeftMovable) return turnL;
                    if (isRightMovable) return turnR;
                }
                else
                {
                    if (currentCommand == turnL) return moveForward;
                    if (isRightMovable) return turnR;
                    if (isLeftMovable) return turnL;
                }
            }

            // Move forward if not turned and forward movable
            return moveForward;
        }
        else
        {
            // Turn if forward unmovable and left or right movable
            if (isLeftMovable) return turnL;
            if (isRightMovable) return turnR;

            // Turn if backward movable
            if (IsMovable(backward))
            {
                return Random.Range(0, 2) == 0 ? turnL : turnR;
            }
        }

        // Idle if unmovable
        return null;
    }

    protected abstract class EnemyCommand : Command
    {
        protected EnemyCommander enemyCommander;

        public EnemyCommand(EnemyCommander commander, float duration) : base(commander, duration)
        {
            enemyCommander = commander;
        }
    }

    protected abstract class MoveCommand : EnemyCommand
    {
        public MoveCommand(EnemyCommander commander, float duration) : base(commander, duration) { }

        protected abstract bool IsMovable { get; }
        protected abstract Vector3 Dest { get; }
        protected Vector3 startPos = default;

        public override void Cancel()
        {
            base.Cancel();
            enemyCommander.ResetOnCharactor(startPos + Dest);
        }

        public override void Execute()
        {
            if (!IsMovable)
            {
                enemyCommander.isCommandValid = true;
                enemyCommander.DispatchCommand();
                return;
            }

            startPos = enemyCommander.tf.position;
            enemyCommander.SetOnCharactor(startPos + Dest);
            enemyCommander.ResetOnCharactor(startPos);

            PlayTweenMove(GetLinearMove(Dest));

            SetValidateTimer(0.95f);
        }
    }

    protected class ForwardCommand : MoveCommand
    {
        public ForwardCommand(EnemyCommander commander, float duration) : base(commander, duration) { }

        protected override bool IsMovable => enemyCommander.IsForwardMovable;
        protected override Vector3 Dest => enemyCommander.dir.LookAt * TILE_UNIT;
        public override float Speed => TILE_UNIT / duration;
    }


    protected class TurnLCommand : EnemyCommand
    {
        public TurnLCommand(EnemyCommander commander, float duration) : base(commander, duration) { }

        public override void Execute()
        {
            PlayTweenMove(GetRotate(-90));
            enemyCommander.TurnLeft();

            SetValidateTimer();
        }
    }

    protected class TurnRCommand : EnemyCommand
    {
        public TurnRCommand(EnemyCommander commander, float duration) : base(commander, duration) { }

        public override void Execute()
        {
            PlayTweenMove(GetRotate(90));
            enemyCommander.TurnRight();

            SetValidateTimer();
        }
    }
}
