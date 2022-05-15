using UnityEngine;

[RequireComponent(typeof(ShieldEnemyReactor))]
[RequireComponent(typeof(ShieldEnemyAnimator))]
[RequireComponent(typeof(EnemyCommandTarget))]
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

    protected bool IsOnPlayer(Pos pos) => MapUtil.IsOnPlayer(pos);
    protected T RandomChoice<T>(params T[] choices) => choices[Random.Range(0, choices.Length)];

    protected override void SetCommands()
    {
        var enemyTarget = target as EnemyCommandTarget;

        die = new EnemyDie(enemyTarget, 72f);
        idle = new EnemyIdle(enemyTarget, 36f);
        moveForward = new EnemyForward(enemyTarget, 72f);
        run = new EnemyForward(enemyTarget, 36f);
        turnL = new ShieldEnemyTurnL(enemyTarget, 16f);
        turnR = new ShieldEnemyTurnR(enemyTarget, 16f);
        guard = new GuardCommand(enemyTarget, 36f, 0.95f);
        attack = new EnemyAttack(enemyTarget, 60f);
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
            switch (Random.Range(0, 3))
            {
                case 0:
                    return currentCommand is EnemyIdle ? attack : idle;
                case 1:
                    return guard;
                default:
                    return idle;
            }
        }

        bool isForwardMovable = mobMap.IsMovable(forward);

        // Move forward if player found in front
        if (mobMap.IsPlayerFound(forward) && isForwardMovable) return run;

        bool isLeftMovable = mobMap.IsMovable(left);
        bool isRightMovable = mobMap.IsMovable(right);

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
            if (mobMap.IsMovable(backward))
            {
                return RandomChoice(turnL, turnR);
            }
        }

        // Idle if unmovable
        return null;
    }

    public virtual void OnActive(bool isSummoned = false)
    {
        ValidateInput();
        if (isSummoned) Interrupt(new EnemySummoned(target as EnemyCommandTarget));
    }
}
