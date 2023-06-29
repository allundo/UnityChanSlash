using UnityEngine;

[RequireComponent(typeof(WitchDoubleAnimator))]
public class WitchDoubleInput : MagicInput
{
    protected ICommand jump;
    protected ICommand backStep;

    protected WitchDoubleReactor react;

    protected override void SetCommands()
    {
        var attack = new WitchDoubleLaunch(target, 32f);
        jump = new WitchDoubleJump(target, 32f, attack);
        backStep = new WitchDoubleBackStep(target, 32f, attack);

        die = new MagicDie(target, 28f);
        react = target.react as WitchDoubleReactor;
    }

    public override void OnActive()
    {
        ValidateInput();
    }

    protected override ICommand GetCommand() => react.IsBackStep ? backStep : jump;
}
