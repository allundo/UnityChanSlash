using UnityEngine;

public interface IEnemyInput : IMobInput
{
    void OnActive(EnemyStatus.ActivateOption option);
}

[RequireComponent(typeof(EnemyCommandTarget))]
public class EnemyAIInput : MobInput, IEnemyInput
{
    protected ICommand turnL;
    protected ICommand turnR;
    protected ICommand moveForward;
    protected ICommand attack;
    protected ICommand doubleAttack;
    protected ICommand fire;

    protected bool IsOnPlayer(Pos pos) => MapUtil.IsOnPlayer(pos);

    public virtual void OnActive(EnemyStatus.ActivateOption option)
    {
        ValidateInput();
        if (option.isSummoned) Interrupt(new EnemySummoned(target as EnemyCommandTarget, option.fadeInDuration * 60f));
    }

    protected override void SetCommands()
    {
        var enemyTarget = target as EnemyCommandTarget;

        die = new EnemyDie(enemyTarget, 72f);
        moveForward = new EnemyForward(enemyTarget, 72f);
        turnL = new EnemyTurnL(enemyTarget, 16f);
        turnR = new EnemyTurnR(enemyTarget, 16f);
        attack = new EnemyAttack(enemyTarget, 100f);
        doubleAttack = new EnemyDoubleAttack(enemyTarget, 100f);
        fire = new EnemyFire(enemyTarget, 108f, enemyTarget.magic?.PrimaryType ?? BulletType.FireBall);
    }

    protected T RandomChoice<T>(params T[] choices) => choices[Random.Range(0, choices.Length)];

    protected override ICommand GetCommand()
    {
        var currentCommand = commander.currentCommand;

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

        // Attack if player found at forward
        Pos forward = mobMap.GetForward;
        if (IsOnPlayer(forward)) return RandomChoice(attack, doubleAttack);

        bool isForwardMovable = mobMap.IsMovable(forward);

        // Move forward if player found in front
        if (mobMap.IsPlayerFound(forward) && isForwardMovable) return moveForward;

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
}
