using UnityEngine;

public interface IEnemyInput : IMobInput
{
    void OnActive(EnemyStatus.ActivateOption option);

    ICommand idle { get; }
    ICommand turnL { get; }
    ICommand turnR { get; }
    ICommand moveForward { get; }
}

public interface IUndeadInput : IEnemyInput
{
    void InterruptSleep();
}

[RequireComponent(typeof(EnemyMapUtil))]
public class EnemyAIInput : MobInput, IEnemyInput
{
    public ICommand idle { get; protected set; }
    public ICommand turnL { get; protected set; }
    public ICommand turnR { get; protected set; }
    public ICommand moveForward { get; protected set; }
    protected ICommand attack;
    protected ICommand doubleAttack;
    protected ICommand fire;
    protected EnemyCommandChoice choice;

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
        turnL = new EnemyTurnL(target, 20f);
        turnR = new EnemyTurnR(target, 20f);
        attack = new EnemyAttack(target, 100f);
        doubleAttack = new EnemyDoubleAttack(target, 100f);
        fire = new EnemyFire(target, 108f, target.magic?.PrimaryType ?? MagicType.FireBall);
    }

    protected override void Start()
    {
        base.Start();
        choice = new EnemyCommandChoice(this);
    }

    protected T RandomChoice<T>(params T[] choices) => EnemyCommandChoice.Random(choices);

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

        return choice.MoveForwardOrTurn(isForwardMovable, isLeftMovable, isRightMovable, mobMap.IsMovable(backward)) ?? idle;
    }
}

public class EnemyCommandChoice
{
    protected IEnemyInput input;
    public EnemyCommandChoice(IEnemyInput input)
    {
        this.input = input;
    }

    public static T Random<T>(params T[] choices) => choices[UnityEngine.Random.Range(0, choices.Length)];

    protected ICommand currentCommand => input.currentCommand;
    protected ICommand idle => input.idle;
    protected ICommand turnL => input.turnL;
    protected ICommand turnR => input.turnR;
    protected ICommand moveForward => input.moveForward;

    public virtual ICommand MoveForwardOrTurn(bool isForwardMovable, bool isLeftMovable, bool isRightMovable, bool isBackwardMovable)
    {
        // Try forwarding with 75% if forward movable
        if (isForwardMovable && Util.DiceRoll(3, 4))
        {
            // Move forward if forward movable and currently not forwarding
            if (currentCommand != moveForward) return moveForward;
            // Move forward if forward movable and currently forwarding with 50%
            if (Util.Judge(2)) return moveForward;
        }

        // Turn to movable direction if not moving forward
        return TurnToMovable(isLeftMovable, isRightMovable, isBackwardMovable) ?? (isForwardMovable ? moveForward : null);
    }

    public virtual ICommand TurnToMovable(bool isLeftMovable, bool isRightMovable, bool isBackwardMovable)
    {
        // Turn if forward unmovable and left or right or backward movable
        ICommand choice = Random(turnL, turnR);
        if (isLeftMovable && isRightMovable)
        {
            if (choice == turnL && !isLeftMovable || currentCommand == turnR) return turnR;
            if (choice == turnR && !isRightMovable || currentCommand == turnL) return turnL;
            return choice;
        }
        if (isLeftMovable && currentCommand != turnR) return turnL;
        if (isRightMovable && currentCommand != turnL) return turnR;
        if (isBackwardMovable) return choice;

        return null;
    }

    public ICommand TurnToViewOpen(bool isForwardViewOpen, bool isLeftViewOpen, bool isRightViewOpen, bool isBackwardViewOpen)
    {
        if (isForwardViewOpen && currentCommand != idle) return idle;
        if (isLeftViewOpen && isRightViewOpen || isBackwardViewOpen) return Random(turnL, turnR);
        if (isLeftViewOpen) return turnL;
        if (isRightViewOpen) return turnR;

        return idle;
    }
}
