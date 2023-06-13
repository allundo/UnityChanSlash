public class LightLaserInput : BulletInput
{
    protected override void SetCommands()
    {
        fire = new LightLaserFire(target, 72f);
        moveForward = new LightLaserActive(target, 24f);
        die = new LightLaserDie(target, 30f);
    }
}
