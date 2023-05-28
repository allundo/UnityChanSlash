using UnityEngine;

[RequireComponent(typeof(ShieldEnemyReactor))]
[RequireComponent(typeof(ShieldEnemyAnimator))]
public class GoblinAIInput : ShieldInput, IEnemyInput
{
    protected ShieldEnemyAnimator shieldAnim;

    protected ICommand idle;
    protected ICommand moveForward;
    protected ICommand run;
    protected ICommand turnL;
    protected ICommand turnR;
    protected ICommand guard;
    protected ICommand attack;

    // Doesn't pay attention to the player if tamed.
    protected bool IsOnPlayer(Pos pos) => !(target.react as IEnemyReactor).IsTamed && MapUtil.IsOnPlayer(pos);
    protected bool IsPlayerFound(Pos pos) => !(target.react as IEnemyReactor).IsTamed && mobMap.IsPlayerFound(pos);
    protected T RandomChoice<T>(params T[] choices) => choices[Random.Range(0, choices.Length)];

    protected override void SetCommands()
    {
        die = new EnemyDie(target, 72f);
        idle = new EnemyIdle(target, 36f);
        moveForward = new EnemyForward(target, 72f);
        run = new EnemyForward(target, 36f);
        turnL = new ShieldEnemyTurnL(target, 16f);
        turnR = new ShieldEnemyTurnR(target, 16f);
        guard = new GuardCommand(target, 36f, 0.95f);
        attack = new EnemyAttack(target, 60f);
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

        // Turn if player found at left, right or backward
        Pos left = mobMap.GetLeft;
        Pos right = mobMap.GetRight;

        if (IsOnPlayer(left)) return turnL;
        if (IsOnPlayer(right)) return turnR;

        Pos backward = mobMap.GetBackward;

        if (IsOnPlayer(backward))
        {
            return RandomChoice(turnL, turnR);
        }

        // Attack or Guard if fighting
        if (shieldAnim.fighting.Bool)
        {
            return RandomChoice(currentCommand is EnemyIdle ? attack : idle, guard, idle);
        }

        bool isForwardMovable = mobMap.IsMovable(forward);

        // Move forward if player found in front
        if (IsPlayerFound(forward) && isForwardMovable) return run;

        return MoveForwardOrTurn(isForwardMovable, mobMap.IsMovable(left), mobMap.IsMovable(right), mobMap.IsMovable(backward)) ?? idle;
    }

    protected virtual ICommand MoveForwardOrTurn(bool isForwardMovable, bool isLeftMovable, bool isRightMovable, bool isBackwardMovable)
    {
        if (isForwardMovable)
        {
            // Turn 50% if left or right movable
            if (Util.Judge(2))
            {
                if (Util.Judge(2))
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
            // Turn if forward unmovable and left or right or backward  movable
            if ((isLeftMovable && isRightMovable) || isBackwardMovable) return RandomChoice(turnL, turnR);
            if (isLeftMovable) return turnL;
            if (isRightMovable) return turnR;

            return null;
        }
    }

    public virtual void OnActive(EnemyStatus.ActivateOption option)
    {
        ValidateInput();
        if (option.isSummoned) Interrupt(new EnemySummoned(target, option.summoningDuration));
    }
}
