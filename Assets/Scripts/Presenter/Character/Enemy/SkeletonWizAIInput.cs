using UnityEngine;

[RequireComponent(typeof(SkeletonWizAnimator))]
[RequireComponent(typeof(SkeletonWizReactor))]
public class SkeletonWizAIInput : EnemyAIInput, IUndeadInput
{
    protected ICommand ice;
    protected ICommand teleport;

    protected UndeadInput undeadInput;

    protected override void SetCommands()
    {
        attack = new EnemyAttack(target, 64f);

        die = new EnemyDie(target, 72f);
        idle = new EnemyIdle(target, 60f);
        moveForward = new EnemyForward(target, 64f);
        turnL = new EnemyTurnAnimL(target, 16f);
        turnR = new EnemyTurnAnimR(target, 16f);

        fire = new EnemyFire(target, 72f, MagicType.DarkHound);
        ice = new EnemyFire(target, 72f, MagicType.IceBullet);
        teleport = new MagicianTeleport(target, 84f, 3);

        undeadInput = new UndeadInput(target, cmd => Interrupt(cmd, true, true));
    }

    public void InterruptSleep() => undeadInput.InterruptSleep();
    public override void OnActive(EnemyStatus.ActivateOption option)
    {
        base.OnActive(option);
        undeadInput.OnActive(option);
    }

    protected override ICommand GetCommand()
    {
        var currentCommand = commander.currentCommand;

        Pos forward = mobMap.GetForward;

        // Start attack if player found at forward
        if (IsOnPlayer(forward)) return RandomChoice(attack, ice, teleport);

        // Turn if player found at left, right or backward
        Pos left = mobMap.GetLeft;
        if (IsOnPlayer(left)) return turnL;

        Pos right = mobMap.GetRight;
        if (IsOnPlayer(right)) return turnR;

        Pos left2 = mobMap.dir.GetLeft(left);
        if (IsOnPlayer(left2)) return turnL;

        Pos right2 = mobMap.dir.GetRight(right);
        if (IsOnPlayer(right2)) return turnR;

        bool isForwardMovable = mobMap.IsMovable(forward);

        if (IsPlayerFound())
        {
            ICommand cmd = isForwardMovable ? RandomChoice(moveForward, fire, ice) : RandomChoice(fire, ice);

            // Replace ice magic with dark hound when player is icing.
            return cmd == ice && PlayerInfo.Instance.IsPlayerIcing ? fire : cmd;
        }

        Pos backward = mobMap.GetBackward;
        if (IsOnPlayer(backward)) return RandomChoice(turnL, turnR);

        bool isLeftMovable = mobMap.IsMovable(left);
        bool isRightMovable = mobMap.IsMovable(right);

        return choice.MoveForwardOrTurn(isForwardMovable, isLeftMovable, isRightMovable, mobMap.IsMovable(backward))
            ?? choice.TurnToViewOpen(IsViewOpen(forward), IsViewOpen(left), IsViewOpen(right), IsViewOpen(backward));
    }
}
