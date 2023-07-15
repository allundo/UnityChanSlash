public class DarkHoundInput : MagicInput
{
    protected override void SetCommands()
    {
        die = new DarkHoundDie(target, 24f);
        fire = new MagicFire(target, fireFrames, die);
        moveForward = new DarkHoundMove(target, moveFrames, die);
    }
}
