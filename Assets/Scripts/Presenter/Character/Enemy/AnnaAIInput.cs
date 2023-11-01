using UnityEngine;

[RequireComponent(typeof(ShieldEnemyReactor))]
[RequireComponent(typeof(ShieldEnemyAnimator))]
[RequireComponent(typeof(EnemyMapUtil))]
public class AnnaAIInput : ShieldInput, IEnemyInput
{
    protected ShieldEnemyAnimator shieldAnim;

    public ICommand idle { get; protected set; }
    public ICommand turnL { get; protected set; }
    public ICommand turnR { get; protected set; }
    public ICommand moveForward { get; protected set; }
    protected ICommand run;
    protected ICommand leftMove;
    protected ICommand rightMove;
    protected ICommand backStep;
    protected ICommand backLeap;
    protected ICommand jump;
    protected ICommand guard;
    protected ICommand attack;
    protected ICommand slash;
    protected ICommand jumpSlash;
    protected ICommand jumpLeapSlash;
    protected ICommand fire;

    protected EnemyCommandChoice choice;

    // Doesn't pay attention to the player if tamed.
    protected bool IsOnPlayer(Pos pos) => !(target.react as IEnemyReactor).IsTamed && map.IsOnPlayer(pos);
    protected bool IsPlayerFound() => !(target.react as IEnemyReactor).IsTamed && mobMap.IsPlayerFound();
    protected bool IsPlayerFound(Pos pos) => !(target.react as IEnemyReactor).IsTamed && map.IsPlayerFound(pos);
    protected T RandomChoice<T>(params T[] choices) => choices[Random.Range(0, choices.Length)];

    protected override void SetCommands()
    {
        var annaTarget = target as AnnaCommandTarget;
        die = new EnemyDie(target, 108f);
        idle = new EnemyIdle(target, 36f);
        moveForward = new AnnaForward(annaTarget, 72f);
        run = new AnnaForward(annaTarget, 36f);
        leftMove = new AnnaLeftMove(annaTarget, 40f);
        rightMove = new AnnaRightMove(annaTarget, 40f);
        backStep = new AnnaBackStep(annaTarget, 40f);
        backLeap = new AnnaBackLeap(annaTarget, 75f, backStep);
        jump = new AnnaJumpLeap(annaTarget, 75f);
        turnL = new ShieldEnemyTurnL(target, 16f);
        turnR = new ShieldEnemyTurnR(target, 16f);
        guard = new GuardCommand(target, 40f, 0.95f);
        attack = new EnemyAttack(target, 40f);
        fire = new EnemyFire(target, 36f, MagicType.FireBall);

        var slash = new AnnaSlash(target, 30f);
        var jumpSlash = new AnnaJumpSlash(target, slash, 54f);
        jumpLeapSlash = new AnnaJumpLeapSlash(target, jumpSlash, 54f);
        this.slash = slash;
        this.jumpSlash = jumpSlash;
    }

    protected override void Start()
    {
        base.Start();
        choice = new EnemyCommandChoice(this);
    }

    protected override void SetInputs()
    {
        guardState = new GuardState(this);
        shieldAnim = target.anim as ShieldEnemyAnimator;
    }

    protected override ICommand GetCommand()
    {
        var currentCommand = commander.currentCommand;

        // Fighting start if player found at forward
        Pos forward = mobMap.GetForward;
        shieldAnim.fighting.Bool = IsOnPlayer(forward);
        shieldAnim.guard.Bool &= shieldAnim.fighting.Bool;

        Pos left = mobMap.GetLeft;
        Pos right = mobMap.GetRight;
        Pos forward2 = mobMap.dir.GetForward(forward);

        bool isLeftMovable = mobMap.IsMovable(left);
        bool isRightMovable = mobMap.IsMovable(right);
        bool isForward2Movable = mobMap.IsMovable(forward2);

        // Attack | Slash | Guard | BackStep | BackLeap if fighting
        if (shieldAnim.fighting.Bool)
        {
            if (currentCommand == idle) return RandomChoice(attack, slash);
            if (!isLeftMovable && !isRightMovable && isForward2Movable) return jump;
            return RandomChoice(guard, idle, attack, backLeap);
        }

        // Turn if player found at left, right or backward
        if (IsOnPlayer(left)) return turnL;

        if (IsOnPlayer(right)) return turnR;

        Pos left2 = mobMap.dir.GetLeft(left);
        if (IsOnPlayer(left2)) return turnL;

        Pos right2 = mobMap.dir.GetRight(right);
        if (IsOnPlayer(right2)) return turnR;

        Pos backward = mobMap.GetBackward;
        if (IsOnPlayer(backward)) return RandomChoice(turnL, turnR);

        Pos backward2 = mobMap.dir.GetBackward(backward);
        if (IsOnPlayer(backward2)) return RandomChoice(turnL, turnR);

        // Left or right move if player found at left-forward or right-forward
        if (isLeftMovable && IsOnPlayer(mobMap.dir.GetForward(left))) return leftMove;
        if (isRightMovable && IsOnPlayer(mobMap.dir.GetForward(right))) return rightMove;

        bool isForwardMovable = mobMap.IsMovable(forward);
        bool isBackwardMovable = mobMap.IsMovable(backward);
        bool isForwardLeapable = mobMap.IsLeapable(forward);

        if (IsPlayerFound())
        {
            if (isForwardMovable) return RandomChoice(fire, fire, jumpLeapSlash, run);

            if (isForwardLeapable)
            {
                if (isForward2Movable) return RandomChoice(fire, jumpLeapSlash);
                if (isLeftMovable && isRightMovable) return RandomChoice(fire, leftMove, rightMove);
                if (isLeftMovable) return RandomChoice(fire, leftMove);
                if (isRightMovable) return RandomChoice(fire, rightMove);
                if (isBackwardMovable) return RandomChoice(fire, backLeap);

                return fire;
            }
        }

        // Try jump with 75% when faced to leapable structure
        if (!isForwardMovable && isForwardLeapable && isForward2Movable && Util.DiceRoll(3, 4)) return jump;

        return choice.MoveForwardOrTurn(isForwardMovable, isLeftMovable, isRightMovable, isBackwardMovable) ?? idle;
    }

    public virtual void OnActive(EnemyStatus.ActivateOption option)
    {
        ValidateInput();
        if (option.isSummoned) Interrupt(new EnemySummoned(target, option.summoningDuration));
    }
}

