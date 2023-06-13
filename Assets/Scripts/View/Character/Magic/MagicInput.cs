public class MagicInput : InputHandler
{
    protected ICommand fire;
    protected ICommand moveForward;

    protected override void SetCommands()
    {
        fire = new MagicFire(target, 28f);
        moveForward = new MagicMove(target, 28f);
        die = new MagicDie(target, 28f);
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
