using UnityEngine;

[RequireComponent(typeof(WitchDoubleAnimator))]
public class WitchDoubleInput : BulletInput
{
    protected ICommand jump;
    protected ICommand backStep;

    protected WitchDoubleReactor react;

    protected override void SetCommands()
    {
        var bulletTarget = target as CommandTarget;

        var attack = new WitchDoubleLaunch(bulletTarget, 32f);
        jump = new WitchDoubleJump(bulletTarget, 32f, attack);
        backStep = new WitchDoubleBackStep(bulletTarget, 32f, attack);

        die = new BulletDie(bulletTarget, 28f);
        react = bulletTarget.react as WitchDoubleReactor;
    }

    public override void OnActive()
    {
        ValidateInput();
    }

    protected override ICommand GetCommand() => react.IsBackStep ? backStep : jump;
}
