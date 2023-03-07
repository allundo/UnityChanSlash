public class BulletInput : InputHandler
{
    protected ICommand fire;
    protected ICommand moveForward;

    protected override void SetCommands()
    {
        fire = new BulletFire(target, 28f);
        moveForward = new BulletMove(target, 28f);
        die = new BulletDie(target, 28f);
    }

    public override void OnActive()
    {
        ValidateInput();
        Interrupt(fire);
    }
    public override ICommand InterruptDie()
    {
        ClearAll();
        Interrupt(die);
        DisableInput();
        return die;
    }
    protected override ICommand GetCommand() => moveForward;
}
