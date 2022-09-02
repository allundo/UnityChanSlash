using UnityEngine;

public interface IEnemyInput : IMobInput
{
    void OnActive(EnemyStatus.ActivateOption option);
}

public class EnemyAIInput : MobInput, IEnemyInput
{
    protected ICommand turnL;
    protected ICommand turnR;
    protected ICommand moveForward;
    protected ICommand attack;
    protected ICommand doubleAttack;
    protected ICommand fire;

    // Doesn't pay attention to the player if tamed.
    protected bool IsOnPlayer(Pos pos) => !(target.react as IEnemyReactor).IsTamed && MapUtil.IsOnPlayer(pos);
    protected bool IsPlayerFound(Pos pos) => !(target.react as IEnemyReactor).IsTamed && mobMap.IsPlayerFound(pos);

    public virtual void OnActive(EnemyStatus.ActivateOption option)
    {
        ValidateInput();
        if (option.isSummoned) Interrupt(new EnemySummoned(target, option.summoningDuration));
    }

    protected override void SetCommands()
    {
        die = new EnemyDie(target, 72f);
        moveForward = new EnemyForward(target, 72f);
        turnL = new EnemyTurnL(target, 16f);
        turnR = new EnemyTurnR(target, 16f);
        attack = new EnemyAttack(target, 100f);
        doubleAttack = new EnemyDoubleAttack(target, 100f);
        fire = new EnemyFire(target, 108f, target.magic?.PrimaryType ?? BulletType.FireBall);
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
        if (IsPlayerFound(forward) && isForwardMovable) return moveForward;

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
