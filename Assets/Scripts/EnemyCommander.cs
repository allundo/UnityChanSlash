using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;

[RequireComponent(typeof(EnemyAnimator))]
public class EnemyCommander : MobCommander
{

    public EnemyAnimator enemyAnim { get; protected set; }
    protected IEnemyAI enemyAI;

    protected override void Awake()
    {
        base.Awake();
        enemyAnim = GetComponent<EnemyAnimator>();
    }

    protected override void SetCommands()
    {
        die = new DieCommand(this, 3.0f);
        enemyAI = new EnemyAI(map, this);
    }

    protected override Command GetCommand()
    {
        return enemyAI.GetCommand();
    }

    public abstract class EnemyCommand : Command
    {
        protected EnemyCommander enemyCommander;
        protected MapUtil map;

        public EnemyCommand(EnemyCommander commander, float duration) : base(commander, duration)
        {
            enemyCommander = commander;
            map = commander.map;
        }
    }

    public abstract class MoveCommand : EnemyCommand
    {
        public MoveCommand(EnemyCommander commander, float duration) : base(commander, duration) { }

        protected abstract bool IsMovable { get; }
        protected abstract Vector3 Dest { get; }
        protected Vector3 startPos = default;

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

            PlayTweenMove(GetLinearMove(Dest));

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
            PlayTweenMove(GetRotate(-90));
            map.TurnLeft();

            SetValidateTimer();
        }
    }

    public class TurnRCommand : EnemyCommand
    {
        public TurnRCommand(EnemyCommander commander, float duration) : base(commander, duration) { }

        public override void Execute()
        {
            PlayTweenMove(GetRotate(90));
            map.TurnRight();

            SetValidateTimer();
        }
    }
    public abstract class EnemyAction : EnemyCommand
    {
        protected MobAnimator.AnimatorTrigger trigger;
        public EnemyAction(EnemyCommander commander, float duration, MobAnimator.AnimatorTrigger trigger) : base(commander, duration)
        {
            this.trigger = trigger;
        }

        public override void Execute()
        {
            trigger.Fire();

            SetValidateTimer();
            SetDispatchFinal();
        }
    }
    public class EnemyAttack : EnemyAction
    {
        public EnemyAttack(EnemyCommander commander, float duration) : base(commander, duration, commander.enemyAnim.attack) { }
    }
}
