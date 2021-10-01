using UnityEngine;

[RequireComponent(typeof(EnemyCommander))]
public class EnemyAIInput : MobInput
{
    private Command turnL;
    private Command turnR;
    private Command moveForward;
    private Command attack;

    private bool IsOnPlayer(Pos pos) => MapUtil.IsOnPlayer(pos);

    protected override void SetCommands()
    {
        var enemyCommander = commander as EnemyCommander;

        die = new DieCommand(enemyCommander, 2.0f);
        moveForward = new EnemyForward(enemyCommander, 2.0f);
        turnL = new EnemyTurnL(enemyCommander, 0.2f);
        turnR = new EnemyTurnR(enemyCommander, 0.2f);
        attack = new EnemyAttack(enemyCommander, 2.0f);
    }

    protected override Command GetCommand()
    {
        var currentCommand = commander.currentCommand;
        Pos pos = map.CurrentPos;

        // Turn if player found at left, right or backward
        Pos left = map.dir.GetLeft(pos);
        Pos right = map.dir.GetRight(pos);

        if (IsOnPlayer(left)) return turnL;
        if (IsOnPlayer(right)) return turnR;

        Pos backward = map.dir.GetBackward(pos);

        if (IsOnPlayer(backward))
        {
            return Random.Range(0, 2) == 0 ? turnL : turnR;
        }

        // Attack if player found at forward
        Pos forward = map.dir.GetForward(pos);
        if (IsOnPlayer(forward)) return attack;

        // Move forward if player found in front
        if (map.IsPlayerFound(forward)) return moveForward;

        bool isForwardMovable = map.IsMovable(forward);
        bool isLeftMovable = map.IsMovable(left);
        bool isRightMovable = map.IsMovable(right);

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
            if (map.IsMovable(backward))
            {
                return Random.Range(0, 2) == 0 ? turnL : turnR;
            }
        }

        // Idle if unmovable
        return null;
    }
}
