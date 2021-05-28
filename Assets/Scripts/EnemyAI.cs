using UnityEngine;

public class EnemyAI : IEnemyAI
{
    private MobCommander.Command turnL;
    private MobCommander.Command turnR;
    private MobCommander.Command moveForward;
    private MobCommander.Command attack;

    private MapUtil map;
    private EnemyCommander commander;

    private bool IsOnPlayer(Pos pos) => MapUtil.IsOnPlayer(pos);

    public EnemyAI(EnemyCommander commander)
    {
        this.commander = commander;
        this.map = commander.map;

        SetCommands();
    }

    public void SetCommands()
    {
        moveForward = new EnemyCommander.ForwardCommand(commander, 2.0f);
        turnL = new EnemyCommander.TurnLCommand(commander, 0.1f);
        turnR = new EnemyCommander.TurnRCommand(commander, 0.1f);
        attack = new EnemyCommander.EnemyAttack(commander, 2.0f);
    }

    public MobCommander.Command GetCommand()
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