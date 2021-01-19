using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;

[RequireComponent(typeof(MobControl))]
[RequireComponent(typeof(Animator))]
public class EnemyCommander : MobCommander
{
    protected Command forward;
    protected Command turnL;
    protected Command turnR;
    protected Command attack;

    protected override void SetCommands()
    {
        forward = new ForwardCommand(this, 2.0f);
        turnL = new TurnLCommand(this, 0.1f);
        turnR = new TurnRCommand(this, 0.1f);
        attack = new AttackCommand(this, 2.0f);
    }

    protected override Command GetCommand()
    {
        switch (Random.Range(0, 4))
        {
            case 0:
                return forward;
            case 1:
                return turnL;
            case 2:
                return turnR;
            case 3:
                return attack;
        }
        return forward;
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

        public override void Execute()
        {
            if (!IsMovable)
            {
                enemyCommander.isCommandValid = true;
                enemyCommander.DispatchCommand();
                return;
            }

            Vector3 startPos = enemyCommander.tf.position;
            enemyCommander.SetOnCharactor(startPos + Dest);

            PlayTweenMove(GetLinearMove(Dest));

            DOVirtual.DelayedCall(duration * 0.25f, () => { enemyCommander.ResetOnCharactor(startPos); });
            DOVirtual.DelayedCall(duration * 0.95f, () => { enemyCommander.isCommandValid = true; });
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

            DOVirtual.DelayedCall(duration * 0.5f, () => { enemyCommander.isCommandValid = true; });
        }
    }

    protected class TurnRCommand : EnemyCommand
    {
        public TurnRCommand(EnemyCommander commander, float duration) : base(commander, duration) { }

        public override void Execute()
        {
            PlayTweenMove(GetRotate(90));
            enemyCommander.TurnRight();

            DOVirtual.DelayedCall(duration * 0.5f, () => { enemyCommander.isCommandValid = true; });
        }
    }
}