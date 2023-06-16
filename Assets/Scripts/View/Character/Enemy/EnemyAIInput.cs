using UnityEngine;

public interface IEnemyInput : IMobInput
{
    void OnActive(EnemyStatus.ActivateOption option);
}

public interface IUndeadInput : IEnemyInput
{
    void InterruptSleep();
}

public class EnemyAIInput : MobInput, IEnemyInput
{
    protected ICommand idle;
    protected ICommand turnL;
    protected ICommand turnR;
    protected ICommand moveForward;
    protected ICommand attack;
    protected ICommand doubleAttack;
    protected ICommand fire;

    // Doesn't pay attention to the player if tamed.
    protected bool IsOnPlayer(Pos pos) => !(target.react as IEnemyReactor).IsTamed && map.IsOnPlayer(pos);
    protected bool IsPlayerFound() => !(target.react as IEnemyReactor).IsTamed && mobMap.IsPlayerFound();
    protected bool IsPlayerFound(Pos pos) => !(target.react as IEnemyReactor).IsTamed && mobMap.IsPlayerFound(pos);
    protected bool IsViewOpen(Pos pos) => mobMap.GetTile(pos).IsViewOpen;

    public virtual void OnActive(EnemyStatus.ActivateOption option)
    {
        ValidateInput();
        if (option.isSummoned) Interrupt(new EnemySummoned(target, option.summoningDuration));
        if (option.icingFrames > 0f) InputIced(option.icingFrames);
    }

    protected override void SetCommands()
    {
        die = new EnemyDie(target, 72f);
        idle = new EnemyIdle(target, 60f);
        moveForward = new EnemyForward(target, 72f);
        turnL = new EnemyTurnL(target, 16f);
        turnR = new EnemyTurnR(target, 16f);
        attack = new EnemyAttack(target, 100f);
        doubleAttack = new EnemyDoubleAttack(target, 100f);
        fire = new EnemyFire(target, 108f, target.magic?.PrimaryType ?? MagicType.FireBall);
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

    protected ICommand TurnToViewOpen(bool isForwardViewOpen, bool isLeftViewOpen, bool isRightViewOpen, bool isBackwardViewOpen)
    {
        if (isForwardViewOpen && currentCommand != idle) return idle;
        if (isLeftViewOpen && isRightViewOpen || isBackwardViewOpen) return RandomChoice(turnL, turnR);
        if (isLeftViewOpen) return turnL;
        if (isRightViewOpen) return turnR;

        return idle;
    }
}
