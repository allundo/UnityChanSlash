using UnityEngine;

public partial class EnemyCommander : MobCommander
{
    [SerializeField] protected MobAttack enemyAttack = default;

    public abstract class EnemyCommand : Command
    {
        protected EnemyCommander enemyCommander;
        protected MapUtil map;
        protected EnemyAnimator anim;

        public EnemyCommand(EnemyCommander commander, float duration) : base(commander, duration)
        {
            enemyCommander = commander;
            map = commander.map;
            anim = commander.anim as EnemyAnimator;
        }
    }

    public abstract class MoveCommand : EnemyCommand
    {
        public MoveCommand(EnemyCommander commander, float duration) : base(commander, duration) { }

        protected abstract bool IsMovable { get; }
        protected abstract Vector3 Dest { get; }
        protected Vector3 startPos = default;
        protected void SetSpeed()
        {
            anim.speed.Float = Speed;
        }

        protected void ResetSpeed()
        {
            anim.speed.Float = 0.0f;
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
                enemyCommander.isCommandValid = true;
                enemyCommander.DispatchCommand();
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

    public class ForwardCommand : MoveCommand
    {
        public ForwardCommand(EnemyCommander commander, float duration) : base(commander, duration) { }

        protected override bool IsMovable => map.IsForwardMovable;
        protected override Vector3 Dest => map.GetForwardVector();
        public override float Speed => TILE_UNIT / duration;
    }

    public class TurnLCommand : EnemyCommand
    {
        public TurnLCommand(EnemyCommander commander, float duration) : base(commander, duration) { }

        public override void Execute()
        {
            PlayTween(tweenMove.GetRotate(-90));
            map.TurnLeft();

            SetValidateTimer();
        }
    }

    public class TurnRCommand : EnemyCommand
    {
        public TurnRCommand(EnemyCommander commander, float duration) : base(commander, duration) { }

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
            anim.attack.Fire();
            playingTween = enemyAttack.SetAttack();

            SetValidateTimer();
            SetDispatchFinal();
        }
    }
}
