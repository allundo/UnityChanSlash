using UnityEngine;

[RequireComponent(typeof(AnnaReactor))]
[RequireComponent(typeof(AnnaAnimator))]
[RequireComponent(typeof(EnemyMapUtil))]
public class AnnaAIInput : ShieldInput, IEnemyInput
{
    protected AnnaAnimator annaAnim;

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
    protected ICommand wakeUp;

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
        leftMove = new AnnaLeftMove(annaTarget, 45f);
        rightMove = new AnnaRightMove(annaTarget, 45f);
        backStep = new AnnaBackStep(annaTarget, 35f);
        backLeap = new AnnaBackLeap(annaTarget, 64f, backStep);
        jump = new AnnaJumpLeap(annaTarget, 64f);
        turnL = new ShieldEnemyTurnL(target, 16f);
        turnR = new ShieldEnemyTurnR(target, 16f);
        guard = new GuardCommand(target, 40f, 0.95f);
        attack = new EnemyAttack(target, 40f);
        fire = new EnemyFire(target, 36f, MagicType.FireBall);
        wakeUp = new AnnaWakeUp(target, 120f);

        var slash = new AnnaSlash(annaTarget, 30f);
        var jumpSlash = new AnnaJumpSlash(annaTarget, slash, 54f);
        jumpLeapSlash = new AnnaJumpLeapSlash(annaTarget, jumpSlash, 54f);
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
        annaAnim = target.anim as AnnaAnimator;
    }

    protected override ICommand GetCommand()
    {
        var currentCommand = commander.currentCommand;

        // Fighting start if player found at forward
        Pos forward = mobMap.GetForward;
        annaAnim.fighting.Bool = !annaAnim.jump.Bool && IsOnPlayer(forward);
        annaAnim.guard.Bool &= annaAnim.fighting.Bool;

        Pos left = mobMap.GetLeft;
        Pos right = mobMap.GetRight;
        Pos forward2 = mobMap.dir.GetForward(forward);
        Pos backward = mobMap.GetBackward;

        bool isLeftMovable = mobMap.IsMovable(left);
        bool isRightMovable = mobMap.IsMovable(right);
        bool isForward2Movable = mobMap.IsMovable(forward2);
        bool isBackwardMovable = mobMap.IsMovable(backward);

        // Attack | Slash | Guard | BackStep | BackLeap if fighting
        if (annaAnim.fighting.Bool)
        {
            if (currentCommand == idle) return RandomChoice(attack, slash);
            if (currentCommand == turnL || currentCommand == turnR) return attack;

            if (!isLeftMovable && !isRightMovable)
            {
                if (currentCommand == run) return attack;
                if (isForward2Movable) return jump;
                if (isBackwardMovable) return backLeap;
            }
            // Try escape from fighting with 25%
            else if (Util.DiceRoll(1, 4))
            {
                if (!isForward2Movable && !isBackwardMovable)
                {
                    if (isLeftMovable && isRightMovable) return RandomChoice(leftMove, rightMove);
                    return isLeftMovable ? leftMove : rightMove;
                }
                else
                {
                    if (isForward2Movable && isBackwardMovable) return RandomChoice(jump, backStep);
                    return isForward2Movable ? jump : backStep;
                }
            }

            return RandomChoice(guard, idle, attack);
        }

        // Turn if player found at left, right or backward
        if (IsOnPlayer(left)) return turnL;

        if (IsOnPlayer(right)) return turnR;

        Pos left2 = mobMap.dir.GetLeft(left);
        if (IsOnPlayer(left2) && map.IsViewable(left)) return turnL;

        Pos right2 = mobMap.dir.GetRight(right);
        if (IsOnPlayer(right2) && map.IsViewable(right)) return turnR;

        if (IsOnPlayer(backward)) return RandomChoice(turnL, turnR);

        Pos backward2 = mobMap.dir.GetBackward(backward);
        if (IsOnPlayer(backward2) && map.IsViewable(backward)) return RandomChoice(turnL, turnR);

        // Left or right move if player found at left-forward or right-forward
        if (currentCommand != rightMove && isLeftMovable && IsOnPlayer(mobMap.dir.GetForward(left))) return leftMove;
        if (currentCommand != leftMove && isRightMovable && IsOnPlayer(mobMap.dir.GetForward(right))) return rightMove;

        bool isForwardMovable = mobMap.IsMovable(forward);
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
        if (option.icingFrames > 0f) InputIced(option.icingFrames);
    }

    public override ICommand InputIced(float icingFrames)
    {
        // Execute iced fall when current height > 0.15f
        if (transform.position.y > 0f)
        {
            ClearAll();
            ICommand iced = new AnnaIcedFall(target, icingFrames, 60f);
            Interrupt(iced);
            commander.EnqueueCommand(wakeUp);
            return iced;
        }

        return base.InputIced(icingFrames);
    }
}

