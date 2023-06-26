public class DarkHoundInput : MagicInput
{
    protected override void SetCommands()
    {
        die = new DarkHoundDie(target, 24f);
        fire = new MagicFire(target, 24f, die);
        moveForward = new DarkHoundMove(target, 12f, die);
    }
}
