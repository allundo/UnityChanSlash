using UnityEngine;

[RequireComponent(typeof(RabbitAnimator))]
[RequireComponent(typeof(RabbitAnimFX))]
[RequireComponent(typeof(RabbitEffect))]
[RequireComponent(typeof(EnemyCommandTarget))]
public class RabbitAIInput : EnemyAIInput
{
    protected ICommand idle;
    protected ICommand wondering;
    protected ICommand jumpAttack;
    protected ICommand somersault;

    protected override void SetCommands()
    {
        var enemyTarget = target as EnemyCommandTarget;

        die = new EnemyDie(enemyTarget, 56f);
        idle = new EnemyIdle(enemyTarget, 32f);
        wondering = new RabbitWondering(enemyTarget, 64f);
        moveForward = new EnemyForward(enemyTarget, 40f);
        turnL = new RabbitTurnL(enemyTarget, 8f);
        turnR = new RabbitTurnR(enemyTarget, 8f);
        attack = new RabbitJumpAttack(enemyTarget, 80f);
        jumpAttack = new RabbitLongJumpAttack(enemyTarget, 80f);
        somersault = new RabbitSomersault(enemyTarget, 80f);
    }
    public void CancelWondering()
    {
        (target.anim as RabbitAnimator).wondering.Bool = false;
        if (commander.currentCommand is RabbitWondering) commander.Cancel();
    }

    protected override ICommand GetCommand()
    {
        var currentCommand = commander.currentCommand;

        Pos forward = map.GetForward;
        Pos left = map.GetLeft;
        Pos right = map.GetRight;
        Pos backward = map.GetBackward;

        bool isForwardMovable = map.IsMovable(forward);
        bool isLeftMovable = map.IsMovable(left);
        bool isRightMovable = map.IsMovable(right);
        bool isBackwardMovable = map.IsMovable(backward);

        // Turn to player if able to somersault or forward isn't movable
        if (IsOnPlayer(left) && (isRightMovable || !isForwardMovable)) return turnL;
        if (IsOnPlayer(right) && (isLeftMovable || !isForwardMovable)) return turnR;

        // Try cowering if player is on backward
        if (IsOnPlayer(backward)) return isForwardMovable ? moveForward : RandomChoice(turnL, turnR);

        // Attack or Somersault if player found at forward
        if (IsOnPlayer(forward))
        {
            if (isBackwardMovable) return RandomChoice(idle, attack, somersault);

            // Sometimes try cowering if backward isn't movable
            if (isLeftMovable && isRightMovable) return RandomChoice(wondering, attack, turnL, turnR);
            if (isLeftMovable) return RandomChoice(wondering, attack, turnL);
            if (isRightMovable) return RandomChoice(wondering, attack, turnR);
            return RandomChoice(wondering, attack);
        }

        Pos backward2 = map.dir.GetBackward(backward);
        Pos left2 = map.dir.GetLeft(left);
        Pos right2 = map.dir.GetRight(right);


        // Turn to player if player is found in 2 tile distance if able to reach player
        if (IsOnPlayer(backward2) && isBackwardMovable) return RandomChoice(turnL, turnR);
        if (IsOnPlayer(left2) && isLeftMovable) return RandomChoice(turnL, moveForward);
        if (IsOnPlayer(right2) && isRightMovable) return RandomChoice(turnR, moveForward);

        Pos forward2 = map.dir.GetForward(forward);

        // Move forward or jump attack if player found in 2 tile distance front
        if (IsOnPlayer(forward2) && isForwardMovable)
        {
            // Sometimes try cowering if left or right is movable
            if (isLeftMovable && isRightMovable) return RandomChoice(moveForward, jumpAttack, turnL, turnR);
            if (isLeftMovable) return RandomChoice(moveForward, jumpAttack, turnL);
            if (isRightMovable) return RandomChoice(moveForward, jumpAttack, turnR);
            return jumpAttack;
        }

        // Move forward if player found in front
        if (map.IsPlayerFound(forward) && isForwardMovable) return moveForward;

        if (isForwardMovable)
        {
            // Inhibit continuous moving forward by wondering
            if (currentCommand == moveForward && Random.Range(0, 2) == 0) return wondering;

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
            if (map.IsMovable(backward))
            {
                return RandomChoice(turnL, turnR);
            }
        }

        // Wonder if unmovable
        return wondering;
    }
}
