public class LightLaserInput : MagicInput
{
    protected override void SetCommands()
    {
        fire = new LightLaserFire(target, 72f);
        moveForward = new LightLaserActive(target, 24f);
        die = new LightLaserDie(target, 30f);
    }
}
