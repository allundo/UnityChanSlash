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
        moveForward = new EnemyForward(target, 64f);
        turnL = new EnemyTurnAnimL(target, 16f);
        turnR = new EnemyTurnAnimR(target, 16f);

        fire = new EnemyFire(target, 72f, BulletType.DarkHound);
        ice = new EnemyFire(target, 72f, BulletType.IceBullet);
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
        if (IsPlayerFound()) return isForwardMovable ? RandomChoice(moveForward, fire, ice) : RandomChoice(fire, ice);

        Pos backward = mobMap.GetBackward;
        if (IsOnPlayer(backward)) return RandomChoice(turnL, turnR);

        return MoveForwardOrTurn(isForwardMovable, mobMap.IsMovable(left), mobMap.IsMovable(right), mobMap.IsMovable(backward))
            ?? TurnToViewOpen(IsViewOpen(left), IsViewOpen(right), IsViewOpen(backward));
    }
}
