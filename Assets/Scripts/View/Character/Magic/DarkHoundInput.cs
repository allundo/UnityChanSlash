public class DarkHoundInput : MagicInput
{
    protected override void SetCommands()
    {
        fire = new MagicFire(target, 24f);
        moveForward = new DarkHoundMove(target, 12f);
        die = new DarkHoundDie(target, 24f);
    }
}
