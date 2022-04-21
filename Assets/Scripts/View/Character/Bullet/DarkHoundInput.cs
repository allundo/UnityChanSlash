public class DarkHoundInput : BulletInput
{
    protected override void SetCommands()
    {
        fire = new BulletFire(target, 24f);
        moveForward = new DarkHoundMove(target, 12f);
        die = new BulletDie(target, 24f);
    }
}
