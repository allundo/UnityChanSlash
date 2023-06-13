using UnityEngine;

[RequireComponent(typeof(WitchDoubleStatus))]
[RequireComponent(typeof(WitchDoubleInput))]
[RequireComponent(typeof(WitchDoubleEffect))]
public class WitchDoubleReactor : MagicReactor
{
    public bool IsBackStep => (status as WitchDoubleStatus).isBackStep;

    protected WitchDoubleEffect witchDoubleEffect;

    protected override void Awake()
    {
        base.Awake();
        witchDoubleEffect = effect as WitchDoubleEffect;
    }

    public void OnAttackStart()
    {
        witchDoubleEffect.OnAttackStart();
    }

    public void OnAttackEnd()
    {
        witchDoubleEffect.OnAttackEnd();
    }
}
