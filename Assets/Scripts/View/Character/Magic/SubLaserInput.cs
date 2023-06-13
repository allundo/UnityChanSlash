public class SubLaserInput : MagicInput
{
    protected override void SetCommands()
    {
        fire = new SubLaserFire(target, 12f);
        moveForward = new LightLaserActive(target, 24f);
        die = new LightLaserDie(target, 18f);
    }
}
