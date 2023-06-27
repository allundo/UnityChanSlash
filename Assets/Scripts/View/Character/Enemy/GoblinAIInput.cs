using UnityEngine;

[RequireComponent(typeof(ShieldEnemyReactor))]
[RequireComponent(typeof(ShieldEnemyAnimator))]
[RequireComponent(typeof(EnemyMapUtil))]
public class GoblinAIInput : ShieldInput, IEnemyInput
{
    protected ShieldEnemyAnimator shieldAnim;

    public ICommand idle { get; protected set; }
    public ICommand turnL { get; protected set; }
    public ICommand turnR { get; protected set; }
    public ICommand moveForward { get; protected set; }
    protected ICommand run;
    protected ICommand guard;
    protected ICommand attack;

    protected EnemyCommandChoice choice;

    // Doesn't pay attention to the player if tamed.
    protected bool IsOnPlayer(Pos pos) => !(target.react as IEnemyReactor).IsTamed && map.IsOnPlayer(pos);
    protected bool IsPlayerFound(Pos pos) => !(target.react as IEnemyReactor).IsTamed && map.IsPlayerFound(pos);
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

        return choice.MoveForwardOrTurn(isForwardMovable, mobMap.IsMovable(left), mobMap.IsMovable(right), mobMap.IsMovable(backward)) ?? idle;
    }

    public virtual void OnActive(EnemyStatus.ActivateOption option)
    {
        ValidateInput();
        if (option.isSummoned) Interrupt(new EnemySummoned(target, option.summoningDuration));
    }
}
